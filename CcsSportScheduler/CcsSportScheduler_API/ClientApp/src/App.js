import React from 'react';
import { AppBar, Toolbar, Typography } from '@mui/material';
import '@syncfusion/ej2-base/styles/material.css';
import '@syncfusion/ej2-react-schedule/styles/material.css';
import 'bootstrap/dist/css/bootstrap.min.css';
import AppRoutes from './AppRoutes';
import './App.css';

const App = () => {
    return (
        <div>
            <AppBar position="static" sx={{ backgroundColor: '#4caf50' }}> {/* Promena boje pozadine na zelenu */}
            </AppBar>
            <AppRoutes />
        </div>
    );
};

export default App;
