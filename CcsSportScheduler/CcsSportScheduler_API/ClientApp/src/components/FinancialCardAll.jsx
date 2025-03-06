import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { MenuItem, Select, Typography, Box, Container, FormControl, InputLabel, Button } from '@mui/material';
import FinancialCard from './FinancialCard';

const FinancialCardAll = ({ user }) => {
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(null);
    const getStartDate = () => {
        const today = new Date();
        const thisYear = today.getFullYear();
        const firstDayOfYear = new Date(thisYear, 0, 1);
        const year = firstDayOfYear.getFullYear();
        return `${year}-01-01T00:00:00`;
    };
    const getEndDate = () => {
        const today = new Date();
        const thisYear = today.getFullYear();
        const firstDayOfYear = new Date(thisYear, 0, 1);
        const year = firstDayOfYear.getFullYear();
        return `${year}-12-31T23:59:59`;
    };
    const [fromDate, setFromDate] = useState(getStartDate());
    const [toDate, setToDate] = useState(getEndDate());

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
                FromDate: fromDate, // Primer datuma, promenite po potrebi
                ToDate: toDate // Primer datuma, promenite po potrebi
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
                FromDate: fromDate, // Primer datuma, promenite po potrebi
                ToDate: toDate // Primer datuma, promenite po potrebi
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