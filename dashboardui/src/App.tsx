import React from "react";
import logo from "./logo.svg";
import "./assets/styles/App.css";
import { Link, Outlet } from "react-router-dom";

function App() {
  return (
    <div className="">
      <header className="">
        <h1>Main</h1>
        <div
          style={{
            display: "flex",
            alignItems: "center",
            width: "100%",
            justifyContent: "space-around",
          }}
        >
          <Link to="/">Home</Link>
          <Link to="/contacts?success=true">Contacts - Visible</Link>
          <Link to="/contacts?success=false">Contacts - Hidden</Link>
        </div>
        <Outlet />
      </header>
    </div>
  );
}

export default App;
