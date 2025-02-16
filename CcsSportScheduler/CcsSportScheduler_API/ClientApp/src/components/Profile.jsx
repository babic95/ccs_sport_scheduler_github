import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { TextField, Button, Typography, Box, Container, Modal, Avatar, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import './Profile.css';

const Profile = ({ user }) => {
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(user);
    const [userData, setUserData] = useState({
        id: user.id || null,
        username: user.username || '',
        fullName: user.fullName || '',
        birthday: user.birthday ? user.birthday.split('T')[0] : '',
        contact: user.contact || '',
        email: user.email || '',
        jmbg: user.jmbg || '',
        profileImageUrl: user.profileImageUrl || ''
    });

    const [financialData, setFinancialData] = useState({
        totalZaduzenje: 0,
        totalRazduzenje: 0,
        items: [],
        totalCount: 0
    });

    const [openPasswordModal, setOpenPasswordModal] = useState(false);
    const [passwordData, setPasswordData] = useState({
        oldPassword: '',
        newPassword: '',
        confirmPassword: ''
    });

    const [selectedFile, setSelectedFile] = useState(null);

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

    useEffect(() => {
        const fetchUserData = async () => {
            try {
                const response = await axios.get(`/api/users/${selectedUser.id}`);
                setUserData({
                    id: selectedUser.id,
                    username: response.data.username || '',
                    fullName: response.data.fullName || '',
                    birthday: response.data.birthday ? response.data.birthday.split('T')[0] : '',
                    contact: response.data.contact || '',
                    email: response.data.email || '',
                    jmbg: response.data.jmbg || '',
                    profileImageUrl: response.data.profileImageUrl || ''
                });
            } catch (error) {
                console.error('Error fetching user data:', error);
            }
        };

        if (selectedUser.id) {
            fetchUserData();
        }

        fetchFinancialData();
    }, [selectedUser]);

    const fetchFinancialData = async () => {
        try {
            const response = await axios.get(`/api/users/financialCard/${selectedUser.id}`);
            setFinancialData(response.data);
        } catch (error) {
            console.error('Error fetching financial data:', error);
        }
    };

    const handleFileChange = (e) => {
        setSelectedFile(e.target.files[0]);
    };

    const handleSave = async () => {
        try {
            if (selectedFile) {
                const formData = new FormData();
                formData.append('file', selectedFile);
                const uploadResponse = await axios.post(`/api/users/uploadProfileImage/${user.id}`, formData, {
                    headers: {
                        'Content-Type': 'multipart/form-data'
                    }
                });

                if (uploadResponse.data && uploadResponse.data.profileImageUrl) {
                    setUserData((prevUserData) => ({
                        ...prevUserData,
                        profileImageUrl: uploadResponse.data.profileImageUrl
                    }));
                }
            }
            alert('Slika uspešno ažurirana!');
        } catch (error) {
            console.error('Error uploading profile image:', error);
            alert('Greška prilikom ažuriranja slike.');
        }
    };

    const handlePasswordChange = (e) => {
        const { name, value } = e.target;
        setPasswordData((prevPasswordData) => ({
            ...prevPasswordData,
            [name]: value
        }));
    };

    const handlePasswordSave = async () => {
        if (passwordData.newPassword !== passwordData.confirmPassword) {
            alert('Nove lozinke se ne podudaraju.');
            return;
        }

        try {
            await axios.post(`/api/users/changePassword/${user.id}`, {
                idUser: user.id,
                oldPassword: passwordData.oldPassword,
                newPassword: passwordData.newPassword
            });
            alert('Lozinka uspešno promenjena!');
            setOpenPasswordModal(false);
        } catch (error) {
            console.error('Error changing password:', error);
            alert('Greška prilikom promene lozinke.');
        }
    };

    const handleUserChange = (event) => {
        const userId = event.target.value;
        const selected = users.find(u => u.id === userId);
        setSelectedUser(selected);
    };

    const saldo = financialData.totalRazduzenje - financialData.totalZaduzenje;

    return (
        <Container component="main" maxWidth="sm" sx={{ mt: 8, mb: 4 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Typography component="h1" variant="h4" color="primary" gutterBottom>
                    Profil
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
                        {users.map(user => (
                            <MenuItem key={user.id} value={user.id}>
                                {user.fullName || user.username}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                {userData.profileImageUrl && (
                    <Box sx={{ mb: 2 }}>
                        <Avatar src={userData.profileImageUrl} alt="Profile" sx={{ width: 100, height: 100 }} />
                    </Box>
                )}

                {selectedUser.id === user.id && (
                    <input
                        type="file"
                        accept="image/*"
                        onChange={handleFileChange}
                        style={{ display: 'block', marginBottom: '1rem' }}
                    />
                )}

                {selectedUser.id === user.id && (
                    <Button
                        type="button"
                        fullWidth
                        variant="contained"
                        color="primary"
                        sx={{ mt: 1, mb: 2 }}
                        onClick={handleSave}
                    >
                        Sačuvaj sliku
                    </Button>
                )}

                {user.type === 6 || selectedUser.id === user.id ? (
                    <>
                        <Typography variant="h6" gutterBottom>
                            Ukupno zaduženje: {financialData.totalZaduzenje} RSD
                        </Typography>
                        <Typography variant="h6" gutterBottom>
                            Ukupno razduženje: {financialData.totalRazduzenje} RSD
                        </Typography>
                        <Typography variant="h6" gutterBottom sx={{ color: saldo < 0 ? 'red' : saldo > 0 ? 'green' : 'inherit' }}>
                            SALDO: {saldo} RSD
                        </Typography>
                    </>
                ) : null}

                <Box component="form" sx={{ mt: 3 }}>
                    <TextField
                        variant="outlined"
                        margin="normal"
                        fullWidth
                        id="username"
                        label="Korisničko ime"
                        name="username"
                        autoComplete="username"
                        value={userData.username || ''}
                        InputLabelProps={{
                            style: { color: '#000000' },
                            shrink: true
                        }}
                        InputProps={{
                            readOnly: true,
                        }}
                    />
                    <TextField
                        variant="outlined"
                        margin="normal"
                        fullWidth
                        id="fullName"
                        label="Puno ime"
                        name="fullName"
                        autoComplete="name"
                        value={userData.fullName || ''}
                        InputLabelProps={{
                            style: { color: '#000000' }
                        }}
                        InputProps={{
                            readOnly: true,
                        }}
                    />
                    <TextField
                        variant="outlined"
                        margin="normal"
                        fullWidth
                        name="birthday"
                        label="Datum rođenja"
                        type="date"
                        id="birthday"
                        InputLabelProps={{
                            shrink: true,
                            style: { color: '#000000' }
                        }}
                        value={userData.birthday || ''}
                        InputProps={{
                            readOnly: true,
                        }}
                    />
                    <TextField
                        variant="outlined"
                        margin="normal"
                        fullWidth
                        id="contact"
                        label="Kontakt"
                        name="contact"
                        autoComplete="contact"
                        value={userData.contact || ''}
                        InputLabelProps={{
                            style: { color: '#000000' }
                        }}
                        InputProps={{
                            readOnly: true,
                        }}
                    />
                    <TextField
                        variant="outlined"
                        margin="normal"
                        fullWidth
                        id="email"
                        label="Email"
                        name="email"
                        autoComplete="email"
                        value={userData.email || ''}
                        InputLabelProps={{
                            style: { color: '#000000' }
                        }}
                        InputProps={{
                            readOnly: true,
                        }}
                    />
                    <TextField
                        variant="outlined"
                        margin="normal"
                        fullWidth
                        id="jmbg"
                        label="JMBG"
                        name="jmbg"
                        autoComplete="jmbg"
                        value={userData.jmbg || ''}
                        InputLabelProps={{
                            style: { color: '#000000' }
                        }}
                        InputProps={{
                            readOnly: true,
                        }}
                    />
                    {selectedUser.id === user.id && (
                        <Button
                            type="button"
                            fullWidth
                            variant="outlined"
                            color="secondary"
                            sx={{ mt: 1 }}
                            onClick={() => setOpenPasswordModal(true)}
                        >
                            Promena lozinke
                        </Button>
                    )}
                </Box>
            </Box>
            <Modal
                open={openPasswordModal}
                onClose={() => setOpenPasswordModal(false)}
                aria-labelledby="modal-modal-title"
                aria-describedby="modal-modal-description"
            >
                <Box sx={{
                    position: 'absolute',
                    top: '50%',
                    left: '50%',
                    transform: 'translate(-50%, -50%)',
                    width: 400,
                    bgcolor: 'background.paper',
                    border: '2px solid #000',
                    boxShadow: 24,
                    p: 4,
                }}>
                    <Typography id="modal-modal-title" variant="h6" component="h2">
                        Promena lozinke
                    </Typography>
                    <TextField
                        margin="normal"
                        required
                        fullWidth
                        name="oldPassword"
                        label="Stara lozinka"
                        type="password"
                        id="oldPassword"
                        autoComplete="current-password"
                        value={passwordData.oldPassword || ''}
                        onChange={handlePasswordChange}
                        InputLabelProps={{
                            style: { color: '#000000' }
                        }}
                    />
                    <TextField
                        margin="normal"
                        required
                        fullWidth
                        name="newPassword"
                        label="Nova lozinka"
                        type="password"
                        id="newPassword"
                        value={passwordData.newPassword || ''}
                        onChange={handlePasswordChange}
                        InputLabelProps={{
                            style: { color: '#000000' }
                        }}
                    />
                    <TextField
                        margin="normal"
                        required
                        fullWidth
                        name="confirmPassword"
                        label="Potvrda nove lozinke"
                        type="password"
                        id="confirmPassword"
                        value={passwordData.confirmPassword || ''}
                        onChange={handlePasswordChange}
                        InputLabelProps={{
                            style: { color: '#000000' }
                        }}
                    />
                    <Button
                        type="button"
                        fullWidth
                        variant="contained"
                        color="primary"
                        sx={{ mt: 3, mb: 2 }}
                        onClick={handlePasswordSave}
                    >
                        Sačuvaj
                    </Button>
                    <Button
                        type="button"
                        fullWidth
                        variant="outlined"
                        color="secondary"
                        sx={{ mt: 1 }}
                        onClick={() => setOpenPasswordModal(false)}
                    >
                        Odustani
                    </Button>
                </Box>
            </Modal>
        </Container>
    );
};

export default Profile;