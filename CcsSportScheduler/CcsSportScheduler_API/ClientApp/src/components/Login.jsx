import React, { useState } from 'react';
import { TextField, Button, Container, Typography, Snackbar } from '@mui/material';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';
import { styled } from '@mui/system';

const CustomTextField = styled(TextField)({
    '& .MuiInputBase-input::placeholder': {
        color: '#000000', // Crni placeholder tekst
        opacity: 1, // Pun opacity za vidljivost
    },
    '& label.Mui-focused': {
        color: '#4caf50', // Zelena boja za labelu
    },
    '& .MuiInput-underline:after': {
        borderBottomColor: '#4caf50', // Zelena boja za underline
    },
    '& .MuiInputLabel-root': {
        color: '#000000', // Crna boja za labelu kada nije fokusirana
    }
});

const Login = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [openSnackbar, setOpenSnackbar] = useState(false);
    const navigate = useNavigate();

    const handleLogin = async (e) => {
        e.preventDefault();
        try {
            const response = await axios.post('/api/users/login', { username, password });
            console.error('response', response);
            if (response.data) {
                navigate('/schedule', { state: { user: response.data } });
                setOpenSnackbar(true);
            } else {
                setError('Login failed');
            }
        } catch (error) {
            console.error('Error during login', error);
            setError('Error during login');
        }
    };

    const handleCloseSnackbar = () => {
        setOpenSnackbar(false);
    };

    return (
        <Container component="main" maxWidth="xs" sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', mt: 8 }}>
            <Typography component="h1" variant="h5" color="primary">Login</Typography>
            <form onSubmit={handleLogin} style={{ width: '100%', mt: 3 }}>
                <CustomTextField
                    margin="normal"
                    required
                    fullWidth
                    label="Username"
                    value={username}
                    onChange={(e) => setUsername(e.target.value)}
                    color="primary"
                />
                <CustomTextField
                    margin="normal"
                    required
                    fullWidth
                    type="password"
                    label="Password"
                    value={password}
                    onChange={(e) => setPassword(e.target.value)}
                    color="primary"
                />
                {error && <Typography color="error" variant="body2">{error}</Typography>}
                <Button type="submit" fullWidth variant="contained" color="primary" sx={{ mt: 3, mb: 2 }}>
                    Login
                </Button>
            </form>
            <Snackbar
                open={openSnackbar}
                autoHideDuration={6000}
                onClose={handleCloseSnackbar}
                message="Successfully logged in!"
                action={
                    <Button color="inherit" size="small" onClick={handleCloseSnackbar}>
                        Close
                    </Button>
                }
            />
        </Container>
    );
};

export default Login;
