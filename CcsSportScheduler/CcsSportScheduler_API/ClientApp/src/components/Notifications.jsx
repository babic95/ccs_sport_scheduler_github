// Notifications.js
import React from 'react';
import { Box, Button, Typography } from '@mui/material';

const Notifications = ({ notifications, handleNotificationClick }) => {
    return (
        <Box sx={{ marginTop: 4 }}>
            <Typography variant="h6" sx={{ marginBottom: 2 }}>Nepročitana Obaveštenja</Typography>
            {notifications.length === 0 ? (
                <Typography>Nema novih obaveštenja</Typography>
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
                        <Box
                            key={notification.id}
                            sx={{
                                padding: 2,
                                marginBottom: 2,
                                border: '1px solid #ccc',
                                borderRadius: '4px',
                                backgroundColor: '#f9f9f9'
                            }}
                        >
                            <Typography>{notification.description}</Typography>
                            <Typography variant="body2" color="textSecondary">
                                {formattedDate}
                            </Typography>
                            <Button
                                variant="contained"
                                color="primary"
                                sx={{ marginTop: 1 }}
                                onClick={() => handleNotificationClick(notification)}
                            >
                                Pregled
                            </Button>
                        </Box>
                    );
                })
            )}
        </Box>
    );
};

export default Notifications;