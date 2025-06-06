﻿import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { MenuItem, Select, Typography, Box, Container, FormControl, InputLabel, Button, TextField } from '@mui/material';
import FinancialCard from './FinancialCard';

const FinancialCardAll = ({ user }) => {
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const getStartDate = () => {
        const today = new Date();
        const thisYear = today.getFullYear();
        const firstDayOfYear = new Date(thisYear, 0, 1);
        const year = firstDayOfYear.getFullYear();
        const month = (firstDayOfYear.getMonth() + 1).toString().padStart(2, '0'); // meseci su 0-indexed, pa dodajemo 1
        const day = firstDayOfYear.getDate().toString().padStart(2, '0');
        return `${year}-${month}-${day}`;
    };
    const getEndDate = () => {
        const today = new Date();
        const thisYear = today.getFullYear();
        const firstDayOfYear = new Date(thisYear, 0, 1);
        const year = firstDayOfYear.getFullYear();
        return `${year}-12-31`;
    };
    const [fromDateReport, setFromDateReport] = useState(getStartDate());
    const [toDateReport, setToDateReport] = useState(getEndDate());

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

    const handlePrintAll = async () => {
        //if (!selectedUser) {
        //    return;
        //}

        try {
            const response = await axios.post('/api/Report/createAll', {
                UserId: 0,
                FromDate: fromDateReport, // Primer datuma, promenite po potrebi
                ToDate: toDateReport // Primer datuma, promenite po potrebi
            }, {
                responseType: 'blob'
            });

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', 'report.pdf');
            document.body.appendChild(link);
            link.click();
        } catch (error) {
            console.error('Error generating PDF:', error);
        }
    };
    const handlePrint = async () => {
        if (!selectedUser) {
            alert('Izaberite korisnika!');
            return;
        }

        try {
            const response = await axios.post('/api/Report/create', {
                UserId: selectedUser.id,
                FromDate: fromDateReport, // Primer datuma, promenite po potrebi
                ToDate: toDateReport // Primer datuma, promenite po potrebi
            }, {
                responseType: 'blob'
            });

            const url = window.URL.createObjectURL(new Blob([response.data]));
            const link = document.createElement('a');
            link.href = url;
            link.setAttribute('download', 'report.pdf');
            document.body.appendChild(link);
            link.click();
        } catch (error) {
            console.error('Error generating PDF:', error);
        }
    };

    return (
        <Container component="main" maxWidth="md" sx={{ mt: 8, mb: 4 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
                    <TextField
                        label="Od datuma"
                        type="date"
                        value={fromDateReport}
                        onChange={(e) => setFromDateReport(e.target.value)}
                        InputLabelProps={{
                            shrink: true,
                            style: { color: 'black' }, // Dodato za crna slova
                        }}
                        sx={{ marginRight: 2 }}
                    />
                    <TextField
                        label="Do datuma"
                        type="date"
                        value={toDateReport}
                        onChange={(e) => setToDateReport(e.target.value)}
                        InputLabelProps={{
                            shrink: true,
                            style: { color: 'black' }, // Dodato za crna slova
                        }}
                    />
                </Box>

                <Button variant="contained" color="primary" onClick={handlePrintAll} sx={{ mt: 3 }}>
                    Štampa svi korisnici
                </Button>
                <Button variant="contained" color="primary" onClick={handlePrint} sx={{ mt: 3 }}>
                    Štampa izabran korisnik
                </Button>
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
                                {user.fullName}({user.username})
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>
                {selectedUser && (
                    <>
                        <FinancialCard key={selectedUser.id} user={selectedUser} />
                    </>
                )}
            </Box>
        </Container>
    );
};

export default FinancialCardAll;