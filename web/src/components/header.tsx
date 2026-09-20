import Link from 'next/link'
import React from 'react'

const Header = () => {
  return (
    <div className="w-full md:max-w-[1600px] md:mx-auto h-header-mobile md:h-header relative z-10 flex flex-row md:justify-between items-center aling-center px-16 ">
      <nav className="md:w-[45%] md:block hidden">
        <ul className="flex justify-evenly align-middle items-center w-full ">
          <li className="inline-block">
            <Link href={"#"}>Link</Link>
          </li>
          <li className="inline-block">
            <Link href={"#"}>Link</Link>
          </li>
          <li className="inline-block">
            <Link href={"#"}>Link</Link>
          </li>
          <li className="inline-block">
            <Link href={"#"}>Link</Link>
          </li>
          <li>
          </li>
        </ul>
      </nav>
    </div>
  )
}

export default Header