import React, { useState, useEffect } from 'react';
import { Route, Routes, useLocation, useNavigate } from 'react-router-dom';
import Schedule from './Schedule';
import Profile from './Profile';
import FinancialCard from './FinancialCard';
import FinancialCardAll from './FinancialCardAll';
import NewNotification from './NewNotification';
import axios from 'axios';
import './Main.css';
import { IconButton, Badge, Menu, MenuItem, Modal, Typography, AppBar, Toolbar, Drawer, List, ListItem, ListItemText, Box, Avatar, Button } from '@mui/material';
import NotificationsIcon from '@mui/icons-material/Notifications';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import MenuIcon from '@mui/icons-material/Menu';
import logo from '../assets/logo.png'; // assuming you have a logo.png in your project

const Header = ({ toggleSidebar, unreadCount, handleNotificationClick, anchorEl, openMenu, closeMenu, notifications, user, navigate }) => (
    <AppBar position="static" sx={{ backgroundColor: '#4caf50' }}>
        <Toolbar>
            <IconButton edge="start" color="inherit" aria-label="menu" onClick={toggleSidebar}>
                <MenuIcon />
            </IconButton>

            <IconButton edge="end" color="inherit" onClick={() => {
                navigate('/schedule', { state: { user } });
            }}>
                <img src={logo} alt="Logo" style={{ width: '40px', height: '40px' }} />
            </IconButton>

            <Box sx={{ flexGrow: 1 }} />
            {user.profileImageUrl && (
                <Avatar src={user.profileImageUrl} alt="Profile" sx={{ width: 40, height: 40, marginRight: 2 }} />
            )}
            <Typography variant="body1" sx={{ color: '#000000', marginRight: 2 }}>
                {user.fullName || 'Korisnik'}
            </Typography>
            <IconButton color="inherit" onClick={openMenu}>
                <Badge badgeContent={unreadCount} color="secondary">
                    <NotificationsIcon />
                </Badge>
            </IconButton>
            <Menu
                anchorEl={anchorEl}
                open={Boolean(anchorEl)}
                onClose={closeMenu}
            >
                {notifications.length === 0 ? (
                    <MenuItem onClick={closeMenu}>Nema novih obaveštenja</MenuItem>
                ) : (
                    notifications.map((notification) => {
                        const formattedDate = new Date(notification.date).toLocaleString('sr-RS', {
                            day: '2-digit',
                            month: '2-digit',
                            year: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit',
                        });
                        return (
                            <MenuItem key={notification.id} onClick={() => handleNotificationClick(notification)}>
                                {notification.description.slice(0, 20)}... <br />
                                <small>{formattedDate}</small>
                            </MenuItem>
                        );
                    })
                )}
            </Menu>
        </Toolbar>
    </AppBar>
);

const Sidebar = ({ isOpen, closeSidebar, navigate, user }) => (
    <Drawer open={isOpen} onClose={closeSidebar}>
        <Box sx={{ width: 250 }}>
            <IconButton onClick={closeSidebar} sx={{ marginLeft: 'auto', color: '#4caf50' }}>
                <ArrowBackIcon />
                <span>Zatvori meni</span>
            </IconButton>
            <List>
                <ListItem button onClick={() => {
                    closeSidebar();
                    navigate('/schedule', { state: { user } });
                }}>
                    <ListItemText primary="Rezervacija Termina" />
                </ListItem>
                {user.type !== 8 && (
                    <ListItem button onClick={() => {
                        closeSidebar();
                        navigate('/profile', { state: { user } });
                    }}>
                        <ListItemText primary="Profil" />
                    </ListItem>
                )}
                {user.type !== 8 && (
                    <ListItem button onClick={() => {
                        closeSidebar();
                        navigate('/financial-card', { state: { user } });
                    }}>
                        <ListItemText primary="Finansijska Kartica" />
                    </ListItem>
                )}
                {user.type === 9 && (
                    <ListItem button onClick={() => {
                        closeSidebar();
                        navigate('/financial-card-all', { state: { user } });
                    }}>
                        <ListItemText primary="Sve Finansijske Kartice" />
                    </ListItem>
                )}
                {user.type === 9 && (
                    <ListItem button onClick={() => {
                        closeSidebar();
                        navigate('/new-notification', { state: { user } });
                    }}>
                        <ListItemText primary="Novo obavestenje" />
                    </ListItem>
                )}
            </List>
        </Box>
        <div className="backdrop" onClick={closeSidebar}></div>
    </Drawer>
);

const Main = () => {
    const location = useLocation();
    const navigate = useNavigate();
    const user = location.state?.user || {};

    const [isSidebarOpen, setSidebarOpen] = useState(false);
    const [unreadCount, setUnreadCount] = useState(0);
    const [notifications, setNotifications] = useState([]);
    const [anchorEl, setAnchorEl] = useState(null);
    const [selectedNotification, setSelectedNotification] = useState(null);

    useEffect(() => {
        const fetchUnreadNotifications = async () => {
            try {
                const response = await axios.get('/api/obavestenjas/nepregledan/1');
                setUnreadCount(response.data ? response.data.length : 0);
                setNotifications(response.data || []);
            } catch (error) {
                console.error('Error fetching notifications:', error);
            }
        };

        fetchUnreadNotifications();
    }, [location.pathname]);

    const toggleSidebar = () => setSidebarOpen(!isSidebarOpen);
    const closeSidebar = () => setSidebarOpen(false);

    const openMenu = (event) => {
        setAnchorEl(event.currentTarget);
    };

    const closeMenu = () => {
        setAnchorEl(null);
    };

    const handleNotificationClick = async (notification) => {
        setSelectedNotification(notification);
        closeMenu();
        try {
            await axios.post(`/api/obavestenjas/seen/${notification.id}`);
            const updatedNotifications = notifications.filter(n => n.id !== notification.id);
            setNotifications(updatedNotifications);
            setUnreadCount(updatedNotifications.length);
        } catch (error) {
            console.error('Error marking notification as seen:', error);
        }
    };

    const handleModalClose = () => {
        setSelectedNotification(null);
    };

    return (
        <>
            <Header
                toggleSidebar={toggleSidebar}
                unreadCount={unreadCount}
                anchorEl={anchorEl}
                openMenu={openMenu}
                closeMenu={closeMenu}
                handleNotificationClick={handleNotificationClick}
                notifications={notifications}
                user={user}
                navigate={navigate}
            />
            <Sidebar isOpen={isSidebarOpen} closeSidebar={closeSidebar} navigate={navigate} user={user} />
            <main className={`main-content ${isSidebarOpen ? 'sidebar-open' : ''}`}>
                <Routes key={location.pathname}>
                    <Route path="/schedule" element={<Schedule user={user} />} />
                    <Route path="/profile" element={<Profile user={user} />} />
                    <Route path="/financial-card" element={<FinancialCard user={user} />} />
                    {user.type === 9 && (
                        <Route path="/financial-card-all" element={<FinancialCardAll user={user} />} />
                    )}
                    {user.type === 9 && (
                        <Route path="/new-notification" element={<NewNotification user={user} />} />
                    )}
                </Routes>
            </main>
            {selectedNotification && (
                <Modal open={Boolean(selectedNotification)} onClose={handleModalClose} aria-labelledby="notification-modal-title" aria-describedby="notification-modal-description">
                    <Box sx={{
                        backgroundColor: '#fff',
                        padding: 4,
                        borderRadius: 2,
                        boxShadow: 24,
                        width: '100%',
                        maxWidth: '600px',
                        position: 'absolute',
                        top: '50%',
                        left: '50%',
                        transform: 'translate(-50%, -50%)'
                    }}>
                        <Typography variant="h6" id="notification-modal-title">Obaveštenje</Typography>
                        <Typography id="notification-modal-description">{selectedNotification.description}</Typography>
                        <Button variant="contained" color="primary" onClick={handleModalClose} sx={{ display: 'block', margin: '20px auto' }}>Zatvori</Button>
                    </Box>
                </Modal>
            )}
        </>
    );
};

export default Main;