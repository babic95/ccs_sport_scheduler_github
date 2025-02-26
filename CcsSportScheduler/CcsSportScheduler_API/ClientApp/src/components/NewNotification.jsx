import React, { useState } from 'react';
import { Box, Button, TextField, Typography } from '@mui/material';
import axios from 'axios';

const NewNotification = () => {
    const [description, setDescription] = useState('');

    const handleSubmit = async () => {
        try {
            const obavestenjeRequest = {
                UserId: 1, // Postavi odgovarajući UserId
                Description: description
            };

            await axios.post('/api/Obavestenjas/sendAllUser', obavestenjeRequest);
            alert('Notification sent successfully');
        } catch (error) {
            console.error('Error sending notification:', error);
            alert('Failed to send notification');
        }
    };

    return (
        <Box sx={{ padding: 2, display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
            <Typography variant="h6" sx={{ marginBottom: 2 }}>Novo Obavestenje</Typography>
            <TextField
                fullWidth
                multiline
                rows={10}
                label="Opis"
                variant="outlined"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                sx={{ marginBottom: 2, maxWidth: '600px' }}
                InputLabelProps={{
                    shrink: true,
                    style: { color: '#000000' }
                }}
            />
            <Button
                variant="contained"
                color="primary"
                onClick={handleSubmit}
                sx={{ alignSelf: 'center' }}
            >
                Pošalji Obavestenje
            </Button>
        </Box>
    );
};

export default NewNotification;