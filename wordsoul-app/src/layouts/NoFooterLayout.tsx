import { Outlet } from "react-router-dom"; // Import Outlet
import Header from "./Header";

export default function NoFooterLayout() {
  return (
    <>
      <Header />
      <Outlet />
    </>
  );
}