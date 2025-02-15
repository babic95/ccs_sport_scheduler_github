import React from 'react';
import { Route, Routes } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import { CssBaseline } from '@mui/material';
import Login from './components/Login';
import Main from './components/Main';

const theme = createTheme({
    palette: {
        primary: {
            main: '#4caf50', // Zelena boja za glavni sadr�aj (npr. trava na terenu)
        },
        secondary: {
            main: '#ff9800', // Narand�asta boja za akcente (npr. �ljaka na terenu)
        },
        background: {
            default: '#f5f5f5', // Svetlo siva za pozadinu aplikacije
        },
        text: {
            primary: '#000000',
            secondary: '#ffffff',
        },
    },
    typography: {
        h5: {
            fontWeight: 600,
        },
        body1: {
            fontSize: '1rem',
        },
    },
});

const AppRoutes = () => (
    <ThemeProvider theme={theme}>
        <CssBaseline />
        <Routes>
            <Route path="/" element={<Login />} />
            <Route path="/*" element={<Main />} />
        </Routes>
    </ThemeProvider>
);

export default AppRoutes;
