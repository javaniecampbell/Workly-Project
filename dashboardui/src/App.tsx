import React from 'react';
import logo from './logo.svg';
import './assets/styles/App.css';
import { Link, Outlet } from 'react-router-dom';

function App() {
  return (
    <div className="">
      <header className="">
              <h1>Main</h1>
              <div>
                  <Link to="/">Home</Link>
                  <Link to="/contacts?success=true">True</Link>
                  <Link to="/contacts?success=false">False</Link>
              </div>
              <Outlet/>
      </header>
    </div>
  );
}

export default App;
