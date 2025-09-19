import { Outlet } from "react-router-dom"; // Import Outlet
import Footer from "./Footer";
import Header from "./Header";

export default function MainLayout() {
  return (
    <>
      <Header />
      <Outlet /> {/* Render nested routes here */}
      <Footer />
    </>
  );
}