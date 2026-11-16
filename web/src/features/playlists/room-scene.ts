import * as THREE from 'three';
import { COVER_COLOURS, PILES, colourFor, type RoomMode } from './shelf';

/**
 * The playlist room: a shelf with four piles of records against a wall, with a window, pictures,
 * a hanging lamp and a plant. By day the sun is outside and the lamp is off; by night the moon
 * is out and the lamp is on.
 *
 * This file is plain three.js with no React in it. playlist-room.tsx owns the <canvas> and the
 * data and calls the few functions returned from createRoomScene. Keeping the two apart means
 * the 3D can be changed, or swapped for something else entirely, without touching the page.
 *
 * Records are drawn in flat colours, not with their real artwork. Spotify's design rules say
 * artwork must not be cropped or distorted, and a cover wrapped round a box in perspective is
 * both. The real covers are in the HTML list beside the scene, flat and whole.
 */

export interface RoomSceneOptions {
  /** Day or night when the room first appears. */
  mode: RoomMode;
  /** True when the visitor asked their system for less motion: no camera drift, no sliding. */
  reducedMotion: boolean;
  /** The record under the pointer (or -1), and where the pointer is, for a tooltip. */
  onHover: (index: number, point: { x: number; y: number } | null) => void;
  onPick: (index: number) => void;
}

export interface RoomScene {
  setRecords: (ids: string[]) => void;
  /** Slides one record forward, e.g. while its row in the list has focus. -1 for none. */
  setHot: (index: number) => void;
  /** Fades the room to day or night. */
  setMode: (mode: RoomMode) => void;
  dispose: () => void;
}

const WALL_Z = -2.6;
const SHELF_X = 1.9;
const SHELF_Z = -2.27;
/** Centre x and floor y of each shelf compartment, in pile order: top row, then bottom row. */
const COMPARTMENTS: [number, number][] = [
  [1.165, 0.905],
  [2.635, 0.905],
  [1.165, 0.06],
  [2.635, 0.06],
];

/** Where the camera stands and looks, for wide and for tall screens. */
const POSES = {
  wide: { position: [2.5, 1.8, 4.1], target: [2.5, 1.65, -2.4], fov: 38 },
  // Further back than the shelf alone would need, so the window stays in the picture.
  tall: { position: [1.4, 1.6, 7.9], target: [1.4, 0, -2.4], fov: 52 },
} as const;

/** A repeatable "random" number from 0 to 1, so the piles look the same on every load. */
function scatter(seed: number): number {
  const x = Math.sin(seed * 127.1 + 311.7) * 43758.5453;
  return x - Math.floor(x);
}

/** Throws if the browser cannot give us WebGL; the caller shows the list on its own instead. */
export function createRoomScene(canvas: HTMLCanvasElement, options: RoomSceneOptions): RoomScene {
  const renderer = new THREE.WebGLRenderer({ canvas, antialias: true });
  // Above 2x the extra pixels cost a lot of battery on phones for no visible gain.
  renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));
  renderer.toneMapping = THREE.ACESFilmicToneMapping;
  renderer.toneMappingExposure = 1.15;
  renderer.shadowMap.enabled = true;
  renderer.shadowMap.type = THREE.PCFSoftShadowMap;

  const scene = new THREE.Scene();
  // One colour object shared by the background and the fog, so the far edges of the room fade
  // into exactly the colour behind them, in either mode.
  const backdrop = new THREE.Color(0x0d0f14);
  const fog = new THREE.Fog(0x0d0f14, 9, 26);
  scene.background = backdrop;
  scene.fog = fog;

  const camera = new THREE.PerspectiveCamera(38, 1, 0.1, 80);
  const anisotropy = Math.min(8, renderer.capabilities.getMaxAnisotropy());

  // WebGL memory is not garbage collected. Everything made here is remembered so dispose() can
  // free it when the visitor leaves the page; otherwise each visit would leak a room.
  const disposables: { dispose: () => void }[] = [];
  const keep = <T extends { dispose: () => void }>(item: T): T => {
    disposables.push(item);
    return item;
  };

  const material = (
    colour: THREE.ColorRepresentation,
    roughness = 0.8,
    metalness = 0,
    extra: THREE.MeshStandardMaterialParameters = {},
  ) => keep(new THREE.MeshStandardMaterial({ color: colour, roughness, metalness, ...extra }));

  const add = (
    parent: THREE.Object3D,
    geometry: THREE.BufferGeometry,
    surface: THREE.Material | THREE.Material[],
    x = 0,
    y = 0,
    z = 0,
    castsShadow = true,
  ) => {
    const mesh = new THREE.Mesh(keep(geometry), surface);
    mesh.position.set(x, y, z);
    mesh.castShadow = castsShadow;
    mesh.receiveShadow = true;
    parent.add(mesh);
    return mesh;
  };

  /** A texture painted in code, so the room needs no image files at all. */
  const painted = (
    width: number,
    height: number,
    paint: (g: CanvasRenderingContext2D, w: number, h: number) => void,
  ) => {
    const surface = document.createElement('canvas');
    surface.width = width;
    surface.height = height;
    const g = surface.getContext('2d');
    if (g) paint(g, width, height);

    const texture = keep(new THREE.CanvasTexture(surface));
    texture.colorSpace = THREE.SRGBColorSpace;
    texture.anisotropy = anisotropy;
    return texture;
  };

  const TAU = Math.PI * 2;

  // ---------- floor and wall ----------

  const floorTexture = painted(512, 512, (g) => {
    g.fillStyle = '#3b2a20';
    g.fillRect(0, 0, 512, 512);
    for (let board = 0; board < 8; board++) {
      const y = board * 64;
      const joint = (board * 173) % 512;
      g.fillStyle = `rgba(${board % 2 ? '255,255,255' : '0,0,0'},${0.03 + ((board * 7) % 5) * 0.012})`;
      g.fillRect(0, y, 512, 64);
      g.fillStyle = 'rgba(0,0,0,0.38)';
      g.fillRect(0, y, 512, 2);
      g.fillRect(joint, y, 2, 64);
      g.fillRect((joint + 260) % 512, y, 2, 64);
    }
  });
  floorTexture.wrapS = floorTexture.wrapT = THREE.RepeatWrapping;
  floorTexture.repeat.set(9, 6);

  const floorMaterial = material(0xffffff, 0.78, 0, { map: floorTexture });
  const floor = add(scene, new THREE.PlaneGeometry(40, 24), floorMaterial, 0, 0, 4, false);
  floor.rotation.x = -Math.PI / 2;
  const wallMaterial = material(0x2a3042, 0.95);
  add(scene, new THREE.PlaneGeometry(40, 9), wallMaterial, 0, 4.5, WALL_Z, false);
  add(scene, new THREE.BoxGeometry(40, 0.14, 0.04), material(0x14161c, 0.7), 0, 0.07, WALL_Z + 0.02, false);

  // ---------- shelf ----------

  const shelfWood = material(0x5a3d2b, 0.7);
  for (const x of [0.43, 1.9, 3.37]) {
    add(scene, new THREE.BoxGeometry(0.06, 1.75, 0.62), shelfWood, x, 0.875, SHELF_Z);
  }
  for (const y of [0.03, 0.875, 1.72]) {
    add(scene, new THREE.BoxGeometry(3.0, 0.06, 0.62), shelfWood, SHELF_X, y, SHELF_Z);
  }
  add(scene, new THREE.BoxGeometry(3.0, 1.75, 0.02), material(0x33241a, 0.9), SHELF_X, 0.875, -2.575);

  /** A plain record sleeve, used for the two leaning on top of the shelf. */
  const sleeveFace = (colour: string) =>
    painted(256, 256, (g) => {
      g.fillStyle = colour;
      g.fillRect(0, 0, 256, 256);
      g.fillStyle = 'rgba(242,240,235,0.16)';
      g.beginPath();
      g.arc(165, 145, 75, 0, TAU);
      g.fill();
      g.fillStyle = 'rgba(13,15,20,0.28)';
      g.fillRect(0, 196, 256, 60);
    });

  const paper = material(0xe8e2d4, 0.9);
  const leaning: [number, number, number, string][] = [
    [0.95, -2.47, 0.07, COVER_COLOURS[1]],
    [1.44, -2.43, -0.08, COVER_COLOURS[3]],
  ];
  for (const [x, z, turn, colour] of leaning) {
    const front = material(0xffffff, 0.75, 0, { map: sleeveFace(colour) });
    const sleeve = add(scene, new THREE.BoxGeometry(0.62, 0.62, 0.012), [paper, paper, paper, paper, front, paper], x, 2.06, z);
    sleeve.rotation.set(-0.13, turn, 0);
  }

  // ---------- pictures on the wall ----------

  const picture = (
    width: number,
    height: number,
    x: number,
    y: number,
    paint: (g: CanvasRenderingContext2D, w: number, h: number) => void,
  ) => {
    const image = painted(512, Math.round((512 * height) / width), paint);
    add(scene, new THREE.BoxGeometry(width + 0.1, height + 0.1, 0.05), material(0x17110d, 0.6), x, y, WALL_Z + 0.03);
    add(scene, new THREE.PlaneGeometry(width, height), material(0xffffff, 0.9, 0, { map: image }), x, y, WALL_Z + 0.058, false);
  };

  picture(1.5, 1.0, 2.6, 2.85, (g, w, h) => {
    g.fillStyle = '#e6dfd0';
    g.fillRect(0, 0, w, h);
    g.fillStyle = '#5b3a6b';
    g.beginPath();
    g.arc(w * 0.36, h * 0.92, w * 0.26, Math.PI, 0);
    g.fill();
    g.fillStyle = '#c9953e';
    g.fillRect(w * 0.62, h * 0.22, w * 0.24, h * 0.7);
    g.fillStyle = '#14161c';
    g.beginPath();
    g.arc(w * 0.74, h * 0.14, w * 0.035, 0, TAU);
    g.fill();
    g.fillRect(w * 0.08, h * 0.92, w * 0.84, 4);
  });

  picture(0.55, 0.75, 1.15, 2.9, (g, w, h) => {
    g.fillStyle = '#2a2230';
    g.fillRect(0, 0, w, h);
    g.fillStyle = '#c9614a';
    for (let stripe = 0; stripe < 5; stripe++) {
      g.fillRect(w * 0.16, h * (0.14 + stripe * 0.16), w * (0.3 + 0.09 * stripe), h * 0.07);
    }
  });

  // ---------- window ----------

  /** What is outside: a skyline under the sun, or under the moon with a few windows lit. */
  const outside = (night: boolean) =>
    painted(512, 768, (g, w, h) => {
      const sky = g.createLinearGradient(0, 0, 0, h);
      sky.addColorStop(0, night ? '#0a1130' : '#7fbdf0');
      sky.addColorStop(1, night ? '#1d2a5c' : '#dcefff');
      g.fillStyle = sky;
      g.fillRect(0, 0, w, h);

      if (night) {
        g.fillStyle = '#f2f0eb';
        for (let star = 0; star < 46; star++) {
          const size = 1.5 + scatter(star + 300) * 2.5;
          g.globalAlpha = 0.5 + scatter(star + 500) * 0.5;
          g.fillRect(scatter(star + 100) * w, scatter(star + 200) * h * 0.6, size, size);
        }
        g.globalAlpha = 1;
      } else {
        // A soft halo, so the sun reads as bright against a pale sky.
        g.fillStyle = 'rgba(255,246,207,0.4)';
        g.beginPath();
        g.arc(w * 0.68, h * 0.2, w * 0.18, 0, TAU);
        g.fill();
      }

      g.fillStyle = night ? '#f3edd2' : '#fff6cf';
      g.beginPath();
      g.arc(w * 0.68, h * 0.2, w * 0.11, 0, TAU);
      g.fill();

      if (night) {
        g.fillStyle = 'rgba(150,150,140,0.5)';
        g.beginPath();
        g.arc(w * 0.65, h * 0.185, w * 0.026, 0, TAU);
        g.arc(w * 0.715, h * 0.225, w * 0.016, 0, TAU);
        g.fill();
      }

      // The skyline: [left edge, height, width], each as a share of the picture.
      const towers = [
        [0, 0.3, 0.2],
        [0.18, 0.42, 0.16],
        [0.32, 0.24, 0.2],
        [0.5, 0.36, 0.18],
        [0.66, 0.2, 0.16],
        [0.8, 0.33, 0.2],
      ];
      towers.forEach(([left, height, width], tower) => {
        const x = left * w;
        const top = h - height * h;
        g.fillStyle = night ? '#070a16' : '#8fa3bb';
        g.fillRect(x, top, width * w, height * h);

        for (let row = 0; row * 26 + 30 < height * h; row++) {
          for (let column = 0; column * 22 + 22 < width * w; column++) {
            const lit = scatter(tower * 97 + row * 13 + column * 7) > 0.62;
            if (night) g.fillStyle = lit ? '#f5c96a' : '#131a30';
            else g.fillStyle = lit ? '#c8d6e6' : '#a6b7ca';
            g.fillRect(x + 12 + column * 22, top + 16 + row * 26, 10, 14);
          }
        }
      });
    });

  const WINDOW_X = -0.45;
  const WINDOW_Y = 2.05;
  const WINDOW_W = 1.2;
  const WINDOW_H = 1.8;

  const windowFrame = material(0xbdb7ab, 0.6);
  add(scene, new THREE.BoxGeometry(WINDOW_W + 0.16, WINDOW_H + 0.16, 0.08), windowFrame, WINDOW_X, WINDOW_Y, WALL_Z + 0.04);

  // The view is not lit by the room (MeshBasicMaterial) and skips tone mapping: it is a light
  // source in the picture, so the lamp must neither dim nor tint it. Night is drawn underneath
  // and day fades in over it, which is all that changing the time of day does to the window.
  const nightView = keep(new THREE.MeshBasicMaterial({ map: outside(true), toneMapped: false }));
  const dayView = keep(new THREE.MeshBasicMaterial({ map: outside(false), toneMapped: false, transparent: true, opacity: 0 }));
  add(scene, new THREE.PlaneGeometry(WINDOW_W, WINDOW_H), nightView, WINDOW_X, WINDOW_Y, WALL_Z + 0.085, false);
  add(scene, new THREE.PlaneGeometry(WINDOW_W, WINDOW_H), dayView, WINDOW_X, WINDOW_Y, WALL_Z + 0.088, false);

  // Glazing bars and the sill.
  add(scene, new THREE.BoxGeometry(WINDOW_W, 0.05, 0.05), windowFrame, WINDOW_X, WINDOW_Y, WALL_Z + 0.1);
  add(scene, new THREE.BoxGeometry(0.04, WINDOW_H, 0.05), windowFrame, WINDOW_X, WINDOW_Y, WALL_Z + 0.1);
  add(scene, new THREE.BoxGeometry(WINDOW_W + 0.34, 0.05, 0.18), windowFrame, WINDOW_X, WINDOW_Y - WINDOW_H / 2 - 0.1, WALL_Z + 0.1);

  // ---------- plant ----------

  const plant = new THREE.Group();
  plant.position.set(4.1, 0, -2.0);
  scene.add(plant);

  add(plant, new THREE.CylinderGeometry(0.21, 0.15, 0.36, 28), material(0x8a4a35, 0.85), 0, 0.18, 0);
  add(plant, new THREE.CylinderGeometry(0.19, 0.19, 0.02, 28), material(0x1a130e, 1), 0, 0.355, 0, false);

  const leafLight = material(0x3e6b4a, 0.7);
  const leafDark = material(0x2f5a3d, 0.7);
  add(plant, new THREE.CylinderGeometry(0.012, 0.016, 1.35, 8), leafDark, 0, 1.03, 0);
  add(plant, new THREE.SphereGeometry(0.075, 16, 12), material(0xc9614a, 0.6), 0, 1.76, 0).scale.set(1, 1.5, 1);

  const leafShape = keep(new THREE.SphereGeometry(1, 12, 8));
  for (let leaf = 0; leaf < 7; leaf++) {
    const stalk = new THREE.Group();
    stalk.position.set(0, 0.42 + leaf * 0.1, 0);
    stalk.rotation.y = leaf * 2.4;
    plant.add(stalk);

    const length = 0.42 + scatter(leaf + 7) * 0.22;
    const lean = 0.45 + scatter(leaf + 17) * 0.45;
    const blade = add(stalk, leafShape, leaf % 2 ? leafLight : leafDark, Math.sin(lean) * length, Math.cos(lean) * length, 0);
    blade.scale.set(0.075, length, 0.018);
    blade.rotation.z = -lean;
  }

  // ---------- lamp and light ----------

  // Moved clear of the window, so the shade does not hang in front of the sky.
  const LAMP_X = 0.55;
  const LAMP_Z = -1.5;
  add(scene, new THREE.CylinderGeometry(0.006, 0.006, 4, 6), material(0x1b1d23, 0.5), LAMP_X, 5.3, LAMP_Z, false);

  const shadeMaterial = material(0xc98f4e, 0.45, 0.6, { side: THREE.DoubleSide, emissive: 0x3a2206 });
  const bulbMaterial = keep(new THREE.MeshBasicMaterial({ color: 0xffe2b0 }));
  add(scene, new THREE.CylinderGeometry(0.1, 0.36, 0.3, 40, 1, true), shadeMaterial, LAMP_X, 3.15, LAMP_Z, false);
  add(scene, new THREE.SphereGeometry(0.07, 16, 12), bulbMaterial, LAMP_X, 3.06, LAMP_Z, false);

  // By night the lamp is the only strong light, which is what makes the room dim at its edges.
  const lamp = new THREE.SpotLight(0xffd3a0, 60, 0, 1.15, 0.85, 2);
  lamp.position.set(LAMP_X, 3.0, LAMP_Z);
  lamp.target.position.set(LAMP_X + 0.2, 0, LAMP_Z - 0.4);
  lamp.castShadow = true;
  lamp.shadow.mapSize.set(1024, 1024);
  lamp.shadow.bias = -0.0006;
  lamp.shadow.camera.near = 0.3;
  lamp.shadow.camera.far = 14;
  scene.add(lamp, lamp.target);

  const glow = new THREE.PointLight(0xffb060, 6, 7, 2);
  glow.position.set(LAMP_X, 3.25, LAMP_Z + 0.1);
  scene.add(glow);

  const ambient = new THREE.HemisphereLight(0x8fa0c8, 0x2a1c14, 1.6);
  scene.add(ambient);

  // A light from where the viewer stands. Without it the lamp alone leaves the inside of the
  // shelf black, and the spines — the whole point of the room — cannot be read.
  const fill = new THREE.DirectionalLight(0xbfd0ff, 1.1);
  fill.position.set(3, 3.5, 7);
  scene.add(fill);

  // By day the sun does the lamp's job, from high on the window's side of the room.
  const sun = new THREE.DirectionalLight(0xfff1d6, 0);
  sun.position.set(-4, 6, 5);
  sun.target.position.set(1.5, 0.5, -2);
  sun.castShadow = true;
  sun.shadow.mapSize.set(1024, 1024);
  sun.shadow.bias = -0.0005;
  sun.shadow.camera.left = -6;
  sun.shadow.camera.right = 6;
  sun.shadow.camera.top = 6;
  sun.shadow.camera.bottom = -3;
  sun.shadow.camera.near = 0.5;
  sun.shadow.camera.far = 24;
  scene.add(sun, sun.target);

  // The patch of light the window throws across the floor: warm by day, blue by night.
  const beam = new THREE.SpotLight(0xffffff, 0, 0, 0.5, 0.9, 2);
  beam.position.set(WINDOW_X, WINDOW_Y + 0.4, WALL_Z + 0.3);
  beam.target.position.set(WINDOW_X + 0.9, 0, 0.2);
  scene.add(beam, beam.target);

  // ---------- day and night ----------

  // Everything that differs between the two. `applyMode(0)` is night, `applyMode(1)` is day, and
  // anything between is a blend, which is how the change fades instead of snapping.
  const NIGHT = {
    backdrop: new THREE.Color(0x0d0f14),
    wall: new THREE.Color(0x2a3042),
    floor: new THREE.Color(1, 1, 1),
    sky: new THREE.Color(0x8fa0c8),
    ground: new THREE.Color(0x2a1c14),
    ambient: 1.6,
    fillColour: new THREE.Color(0xbfd0ff),
    fill: 1.1,
    lamp: 60,
    glow: 6,
    sun: 0,
    beamColour: new THREE.Color(0x8fa8ff),
    beam: 12,
    exposure: 1.15,
    shadeGlow: new THREE.Color(0x3a2206),
    bulb: new THREE.Color(0xffe2b0),
  };
  const DAY = {
    backdrop: new THREE.Color(0xcfc8ba),
    wall: new THREE.Color(0xd6cebf),
    // Above 1 on purpose: the boards are painted dark for the night room, and daylight alone
    // leaves them almost black. This lifts the same wood rather than swapping its texture.
    floor: new THREE.Color(2.3, 2.2, 2.1),
    sky: new THREE.Color(0xffffff),
    ground: new THREE.Color(0xb8a58c),
    ambient: 2.4,
    fillColour: new THREE.Color(0xfff4e0),
    fill: 1.3,
    lamp: 0,
    glow: 0,
    sun: 2.2,
    beamColour: new THREE.Color(0xfff1d6),
    beam: 26,
    exposure: 0.95,
    shadeGlow: new THREE.Color(0x000000),
    bulb: new THREE.Color(0x8d897f),
  };

  const blend = (night: number, day: number, amount: number) => night + (day - night) * amount;

  const applyMode = (amount: number) => {
    backdrop.lerpColors(NIGHT.backdrop, DAY.backdrop, amount);
    fog.color.copy(backdrop);
    wallMaterial.color.lerpColors(NIGHT.wall, DAY.wall, amount);
    floorMaterial.color.lerpColors(NIGHT.floor, DAY.floor, amount);

    ambient.color.lerpColors(NIGHT.sky, DAY.sky, amount);
    ambient.groundColor.lerpColors(NIGHT.ground, DAY.ground, amount);
    ambient.intensity = blend(NIGHT.ambient, DAY.ambient, amount);
    fill.color.lerpColors(NIGHT.fillColour, DAY.fillColour, amount);
    fill.intensity = blend(NIGHT.fill, DAY.fill, amount);

    lamp.intensity = blend(NIGHT.lamp, DAY.lamp, amount);
    glow.intensity = blend(NIGHT.glow, DAY.glow, amount);
    sun.intensity = blend(NIGHT.sun, DAY.sun, amount);
    beam.color.lerpColors(NIGHT.beamColour, DAY.beamColour, amount);
    beam.intensity = blend(NIGHT.beam, DAY.beam, amount);

    // A light that is fully off is hidden, so no time is spent drawing shadows for it.
    lamp.visible = amount < 1;
    glow.visible = amount < 1;
    sun.visible = amount > 0;

    shadeMaterial.emissive.lerpColors(NIGHT.shadeGlow, DAY.shadeGlow, amount);
    bulbMaterial.color.lerpColors(NIGHT.bulb, DAY.bulb, amount);
    dayView.opacity = amount;
    renderer.toneMappingExposure = blend(NIGHT.exposure, DAY.exposure, amount);
  };

  let modeGoal = options.mode === 'day' ? 1 : 0;
  let modeNow = modeGoal;
  applyMode(modeNow);

  // ---------- records ----------

  const recordGroup = new THREE.Group();
  scene.add(recordGroup);

  // One box shape for every record; each mesh only stretches it a little.
  const recordShape = keep(new THREE.BoxGeometry(1, 0.046, 0.5));
  const hitMaterial = keep(new THREE.MeshBasicMaterial());
  const recordMaterials = new Map<string, THREE.Material[]>();

  const materialsFor = (colour: string) => {
    const cached = recordMaterials.get(colour);
    if (cached) return cached;

    // The spine: the cover colour with a pale paper label, as on the edge of a CD case.
    const spine = painted(512, 32, (g) => {
      g.fillStyle = colour;
      g.fillRect(0, 0, 512, 32);
      g.fillStyle = 'rgba(242,240,235,0.85)';
      g.fillRect(70, 8, 150, 16);
      g.fillStyle = 'rgba(13,15,20,0.55)';
      g.fillRect(84, 14, 90, 4);
      g.fillStyle = 'rgba(13,15,20,0.35)';
      g.fillRect(0, 0, 512, 2);
      g.fillRect(0, 30, 512, 2);
    });

    const front = material(0xffffff, 0.7, 0, { map: spine });
    const side = material(new THREE.Color(colour).multiplyScalar(0.55), 0.8);
    const top = material(colour, 0.7);
    // Box faces in three.js order: right, left, top, bottom, front, back.
    const faces = [side, side, top, side, front, side];
    recordMaterials.set(colour, faces);
    return faces;
  };

  interface ShelfRecord {
    mesh: THREE.Mesh;
    restZ: number;
    /** 0 = at rest, 1 = pulled fully forward. Eased towards its goal every frame. */
    out: number;
  }

  let records: ShelfRecord[] = [];
  // Invisible copies that never move. The pointer is tested against these, not the visible
  // records: a hovered record slides forward, and if that same record were the thing being
  // tested, sliding it would change which one is under the pointer and the hover would flicker.
  let hitboxes: THREE.Mesh[] = [];
  let hot = -1;

  const setRecords = (ids: string[]) => {
    recordGroup.clear();
    records = [];
    hitboxes = [];
    hot = -1;

    let index = 0;
    PILES.forEach((capacity, pile) => {
      // A pile is built from the shelf up, so count how many of this pile's slots are used.
      const count = Math.max(0, Math.min(capacity, ids.length - index));
      const [centreX, floorY] = COMPARTMENTS[pile];

      for (let slot = 0; slot < count; slot++, index++) {
        const mesh = new THREE.Mesh(recordShape, materialsFor(colourFor(ids[index])));
        mesh.scale.x = 0.98 + scatter(index) * 0.2;
        mesh.position.set(
          centreX + (scatter(index + 40) - 0.5) * 0.14,
          // First record of the pile on top, like a stack you read downwards.
          floorY + 0.025 + (count - 1 - slot) * 0.05,
          SHELF_Z + 0.04,
        );
        mesh.rotation.y = (scatter(index + 80) - 0.5) * 0.1;
        mesh.castShadow = true;
        mesh.receiveShadow = true;
        recordGroup.add(mesh);
        records.push({ mesh, restZ: mesh.position.z, out: 0 });

        const hitbox = new THREE.Mesh(recordShape, hitMaterial);
        hitbox.scale.copy(mesh.scale);
        hitbox.position.copy(mesh.position);
        hitbox.rotation.copy(mesh.rotation);
        hitbox.visible = false;
        hitbox.userData.index = index;
        recordGroup.add(hitbox);
        hitboxes.push(hitbox);
      }
    });
  };

  // ---------- camera ----------

  const cameraPosition = new THREE.Vector3();
  const cameraTarget = new THREE.Vector3();

  const resize = () => {
    const width = canvas.clientWidth;
    const height = canvas.clientHeight;
    if (!width || !height) return;

    renderer.setSize(width, height, false);
    camera.aspect = width / height;

    const pose = camera.aspect < 0.85 ? POSES.tall : POSES.wide;
    cameraPosition.fromArray(pose.position);
    cameraTarget.fromArray(pose.target);
    camera.fov = pose.fov;
    camera.updateProjectionMatrix();
  };

  // ---------- pointer ----------

  const pointer = new THREE.Vector2(0, 0);
  const drift = new THREE.Vector2(0, 0);
  const raycaster = new THREE.Raycaster();

  const recordAt = (event: PointerEvent | MouseEvent) => {
    const bounds = canvas.getBoundingClientRect();
    pointer.set(
      ((event.clientX - bounds.left) / bounds.width) * 2 - 1,
      -((event.clientY - bounds.top) / bounds.height) * 2 + 1,
    );
    raycaster.setFromCamera(pointer, camera);
    const hit = raycaster.intersectObjects(hitboxes, false)[0];
    return hit ? (hit.object.userData.index as number) : -1;
  };

  const onPointerMove = (event: PointerEvent) => {
    const index = recordAt(event);
    hot = index;
    canvas.style.cursor = index >= 0 ? 'pointer' : '';
    // A tooltip follows a mouse; under a finger it would only sit beneath the fingertip.
    const point = index >= 0 && event.pointerType === 'mouse' ? { x: event.clientX, y: event.clientY } : null;
    options.onHover(index, point);
  };

  const onPointerLeave = () => {
    hot = -1;
    pointer.set(0, 0);
    canvas.style.cursor = '';
    options.onHover(-1, null);
  };

  const onClick = (event: MouseEvent) => {
    const index = recordAt(event);
    if (index >= 0) options.onPick(index);
  };

  canvas.addEventListener('pointermove', onPointerMove);
  canvas.addEventListener('pointerleave', onPointerLeave);
  canvas.addEventListener('click', onClick);

  // ---------- drawing ----------

  let frame = 0;
  let previous = performance.now();

  const draw = (now: number) => {
    const elapsed = Math.min(0.05, (now - previous) / 1000);
    previous = now;

    if (modeNow !== modeGoal) {
      modeNow += (modeGoal - modeNow) * (options.reducedMotion ? 1 : Math.min(1, elapsed * 3.5));
      // Close enough is finished: without this the fade would creep towards its goal forever.
      if (Math.abs(modeGoal - modeNow) < 0.004) modeNow = modeGoal;
      applyMode(modeNow);
    }

    records.forEach((record, index) => {
      const goal = index === hot ? 1 : 0;
      record.out = options.reducedMotion ? goal : record.out + (goal - record.out) * Math.min(1, elapsed * 10);
      record.mesh.position.z = record.restZ + record.out * 0.36;
    });

    // The camera leans a little towards the pointer, which is what gives the room its depth.
    const lean = options.reducedMotion ? 0 : 1;
    drift.x += (pointer.x * lean - drift.x) * Math.min(1, elapsed * 3);
    drift.y += (pointer.y * lean - drift.y) * Math.min(1, elapsed * 3);
    camera.position.set(cameraPosition.x + drift.x * 0.14, cameraPosition.y + drift.y * 0.07, cameraPosition.z);
    camera.lookAt(cameraTarget);

    renderer.render(scene, camera);
    frame = requestAnimationFrame(draw);
  };

  // A hidden tab should not keep the graphics card busy.
  const onVisibilityChange = () => {
    cancelAnimationFrame(frame);
    if (!document.hidden) {
      previous = performance.now();
      frame = requestAnimationFrame(draw);
    }
  };
  document.addEventListener('visibilitychange', onVisibilityChange);

  const resizeObserver = new ResizeObserver(resize);
  resizeObserver.observe(canvas);
  resize();
  frame = requestAnimationFrame(draw);

  return {
    setRecords,
    setHot: (index) => {
      hot = index;
    },
    setMode: (mode) => {
      modeGoal = mode === 'day' ? 1 : 0;
    },
    dispose: () => {
      cancelAnimationFrame(frame);
      resizeObserver.disconnect();
      document.removeEventListener('visibilitychange', onVisibilityChange);
      canvas.removeEventListener('pointermove', onPointerMove);
      canvas.removeEventListener('pointerleave', onPointerLeave);
      canvas.removeEventListener('click', onClick);
      disposables.forEach((item) => item.dispose());
      renderer.dispose();
    },
  };
}
