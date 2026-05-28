"use client";

import { useRef } from "react";
import { Canvas, useFrame } from "@react-three/fiber";
import {
  RoundedBox,
  OrbitControls,
  ContactShadows,
  PerspectiveCamera,
} from "@react-three/drei";
import * as THREE from "three";
import { usePlayerStore } from "@/store/playerStore";

function Platter({ isPlaying }: { isPlaying: boolean }) {
  const platterRef = useRef<THREE.Group>(null);

  useFrame((_, delta) => {
    if (platterRef.current && isPlaying) {
      platterRef.current.rotation.y += delta * 1.6;
    }
  });

  return (
    <group ref={platterRef} position={[0, 0.18, 0]}>
      {/* slipmat / platter top */}
      <mesh castShadow receiveShadow>
        <cylinderGeometry args={[1.55, 1.55, 0.12, 64]} />
        <meshStandardMaterial color="#0c0c0c" roughness={0.85} metalness={0.1} />
      </mesh>
      {/* subtle ring detail so rotation is visible */}
      <mesh position={[0.9, 0.07, 0]}>
        <boxGeometry args={[0.04, 0.02, 0.6]} />
        <meshStandardMaterial color="#2a2a2a" />
      </mesh>
    </group>
  );
}

function Spindle() {
  return (
    <mesh position={[0, 0.32, 0]}>
      <cylinderGeometry args={[0.04, 0.04, 0.2, 16]} />
      <meshStandardMaterial color="#dddddd" metalness={0.4} roughness={0.35} />
    </mesh>
  );
}

function Tonearm({ isPlaying, accent }: { isPlaying: boolean; accent: string }) {
  const armRef = useRef<THREE.Group>(null);
  const restY = -0.35;
  const playY = 0.35;

  useFrame((_, delta) => {
    if (!armRef.current) return;
    const target = isPlaying ? playY : restY;
    armRef.current.rotation.y = THREE.MathUtils.damp(
      armRef.current.rotation.y,
      target,
      4,
      delta
    );
  });

  return (
    <group position={[1.7, 0.3, -1.5]}>
      {/* pivot base */}
      <mesh position={[0, 0, 0]} castShadow>
        <cylinderGeometry args={[0.18, 0.2, 0.3, 24]} />
        <meshStandardMaterial color="#2a2a2a" metalness={0.3} roughness={0.5} />
      </mesh>
      {/* arm swings around the pivot */}
      <group ref={armRef} position={[0, 0.18, 0]}>
        <mesh position={[-0.9, 0, 0.5]} castShadow>
          <boxGeometry args={[1.9, 0.05, 0.05]} />
          <meshStandardMaterial color="#cacaca" metalness={0.3} roughness={0.4} />
        </mesh>
        {/* headshell */}
        <mesh position={[-1.8, -0.04, 0.5]} castShadow>
          <boxGeometry args={[0.18, 0.12, 0.16]} />
          <meshStandardMaterial color={accent} metalness={0.3} roughness={0.5} />
        </mesh>
      </group>
    </group>
  );
}

function Scene() {
  const isPlaying = usePlayerStore((s) => s.isPlaying);
  const currentVinyl = usePlayerStore((s) => s.currentVinyl);
  const accent = currentVinyl?.accentColor ?? "#e8b04b";

  return (
    <>
      <PerspectiveCamera makeDefault position={[0, 4.2, 5.2]} fov={42} />
      <ambientLight intensity={0.45} />
      <directionalLight
        position={[4, 8, 4]}
        intensity={1.3}
        castShadow
        shadow-mapSize={[1024, 1024]}
      />
      <spotLight position={[-5, 6, 2]} intensity={0.6} angle={0.5} penumbra={0.6} />

      {/* plinth */}
      <RoundedBox args={[5, 0.5, 4]} radius={0.12} smoothness={4} position={[0, -0.1, 0]} castShadow receiveShadow>
        <meshStandardMaterial color="#241c15" roughness={0.6} metalness={0.2} />
      </RoundedBox>

      <Platter isPlaying={isPlaying} />
      <Spindle />
      <Tonearm isPlaying={isPlaying} accent={accent} />

      <ContactShadows position={[0, -0.36, 0]} opacity={0.5} scale={12} blur={2.4} far={4} />
      <OrbitControls
        enableZoom={false}
        enablePan={false}
        minPolarAngle={Math.PI / 4}
        maxPolarAngle={Math.PI / 2.3}
        autoRotate={!isPlaying}
        autoRotateSpeed={0.6}
      />
    </>
  );
}

const Turntable3D = () => {
  return (
    <Canvas shadows dpr={[1, 2]} className="!absolute inset-0">
      <Scene />
    </Canvas>
  );
};

export default Turntable3D;
