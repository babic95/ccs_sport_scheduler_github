import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { MenuItem, Select, Typography, Box, Container, FormControl, InputLabel } from '@mui/material';
import FinancialCard from './FinancialCard';

const FinancialCardAll = ({ user }) => {
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);

    useEffect(() => {
        const fetchUsers = async () => {
            try {
                const response = await axios.get('/api/users/getAllUsersFromKlub/1');
                setUsers(response.data);
            } catch (error) {
                console.error('Error fetching users:', error);
            }
        };

        fetchUsers();
    }, []);

    const handleUserChange = (event) => {
        const userId = event.target.value;
        const user = users.find(u => u.id === userId);
        setSelectedUser(user);
    };

    return (
        <Container component="main" maxWidth="md" sx={{ mt: 8, mb: 4 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Typography component="h1" variant="h4" color="primary" gutterBottom>
                    Pregled Finansijskih Kartica
                </Typography>
                <FormControl fullWidth variant="outlined" sx={{ mb: 3 }}>
                    <InputLabel id="user-select-label" sx={{ color: '#000000' }}>Izaberite korisnika</InputLabel>
                    <Select
                        labelId="user-select-label"
                        value={selectedUser ? selectedUser.id : ''}
                        onChange={handleUserChange}
                        label="Izaberite korisnika"
                        sx={{ color: '#000000' }}
                    >
                        <MenuItem value="" disabled>
                            Izaberite korisnika
                        </MenuItem>
                        {users.map(user => (
                            <MenuItem key={user.id} value={user.id}>
                                {user.fullName || user.username}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                {selectedUser && (
                    <FinancialCard key={selectedUser.id} user={selectedUser} />
                )}
            </Box>
        </Container>
    );
};

export default FinancialCardAll;