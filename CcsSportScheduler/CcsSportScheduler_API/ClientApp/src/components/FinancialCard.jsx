﻿import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { Typography, Box, Container, Card, CardContent, Modal, Button, FormControl, InputLabel, Select, MenuItem, TextField } from '@mui/material';
import Pagination from '@mui/material/Pagination';

const FinancialCard = ({ user }) => {
    const userId = user.id;
    const [financialData, setFinancialData] = useState({
        totalZaduzenje: 0,
        totalRazduzenje: 0,
        items: [],
        totalCount: 0
    });

    const [selectedItem, setSelectedItem] = useState(null);
    const [modalContent, setModalContent] = useState('');
    const [openModal, setOpenModal] = useState(false);
    const [filterType, setFilterType] = useState('-1');
    const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);
    const [page, setPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);

    const getCurrentYearStart = () => {
        const today = new Date();
        const thisYear = today.getFullYear();
        const firstDayOfYear = new Date(thisYear, 0, 1);
        const year = firstDayOfYear.getFullYear();
        const month = (firstDayOfYear.getMonth() + 1).toString().padStart(2, '0'); // meseci su 0-indexed, pa dodajemo 1
        const day = firstDayOfYear.getDate().toString().padStart(2, '0');
        return `${year}-${month}-${day}`;
    };
    const [fromDate, setFromDate] = useState(getCurrentYearStart());

    const fetchFinancialData = async () => {
        try {
            const response = await axios.get(`/api/users/financialCard/${userId}/${page}/20?fromDate=${fromDate}&toDate=${toDate}&type=${filterType}`);
            setFinancialData(response.data);
            console.log(response.data);
            setTotalPages(Math.ceil(response.data.totalCount / 20));
        } catch (error) {
            console.error('Error fetching financial data:', error);
        }
    };

    useEffect(() => {
        fetchFinancialData();
    }, [userId, filterType, fromDate, toDate, page]);

    const handleCardClick = async (item) => {
        setSelectedItem(item);
        if (item.type === 0) {
            const response = await axios.get(`/api/racuns/${item.id}`);
            const racun = response.data;
            setModalContent(
                <div>
                    <Typography>Broj računa: {racun.invoiceNumber}</Typography>
                    <Typography>Datum: {new Date(racun.date).toLocaleDateString('sr-RS')}</Typography>
                    <Typography>Tip: Kafić"</Typography>
                    <Typography>Ukupan iznos: {racun.totalAmount} RSD</Typography>
                    <Typography>Plaćeno: {racun.placeno === 0 ? racun.pretplata : racun.placeno} RSD</Typography>
                    <Typography>Otpisano: {racun.otpis} RSD</Typography>
                    <Typography>Stavke:</Typography>
                    <ul>
                        {racun.racunitems.map((item, index) => (
                            <li key={index}>
                                Naziv: {item.name}<br />
                                Količina: {item.quantity}<br />
                                Jedinična cena: {item.unitPrice} RSD<br />
                                Ukupno: {item.totalAmount} RSD
                            </li>
                        ))}
                    </ul>
                </div>
            );
        } else if (item.type === 1) {
            const response = await axios.get(`/api/termins/${item.id}`);
            const termin = response.data;
            setModalContent(
                <div>
                    <Typography>Teren: {termin.teren.name}</Typography>
                    <Typography>Datum početka: {new Date(termin.startDateTime).toLocaleString('sr-RS')}</Typography>
                    <Typography>Datum završetka: {new Date(termin.endDateTime).toLocaleString('sr-RS')}</Typography>
                    <Typography>Cena: {termin.price} RSD</Typography>
                    <Typography>Plaćeno: {termin.placeno} RSD</Typography>
                    <Typography>Otpisano: {termin.otpis} RSD</Typography>
                </div>
            );
        } else if (item.type === 2) {
            const response = await axios.get(`/api/uplatas/${item.id}`);
            const uplata = response.data;
            setModalContent(
                <div>
                    <Typography>Ukupan iznos: {uplata.totalAmount} RSD</Typography>
                    <Typography>Razduženo: {uplata.razduzeno} RSD</Typography>
                    <Typography>Datum: {new Date(uplata.date).toLocaleDateString('sr-RS')}</Typography>
                </div>
            );
        } else if (item.type === 3) {
            const response = await axios.get(`/api/uplatas/${item.id}`);
            const poklon = response.data;

            console.log(poklon);
            setModalContent(
                <div>
                    <Typography>Ukupan iznos: {poklon.totalAmount} RSD</Typography>
                    <Typography>Otpisano: {poklon.razduzeno} RSD</Typography>
                    <Typography>Datum: {new Date(poklon.date).toLocaleDateString('sr-RS')}</Typography>
                    <Typography>Opis: {poklon.description}</Typography>
                </div>
            );
        } else if (item.type === 4) {
            const response = await axios.get(`/api/zaduzenjes/${item.id}`);
            const kotizacija = response.data;
            setModalContent(
                <div>
                    <Typography>Datum: {new Date(kotizacija.date).toLocaleDateString('sr-RS')}</Typography>
                    <Typography>Opis: {kotizacija.opis}</Typography>
                    <Typography>Ukupan iznos: {kotizacija.totalAmount} RSD</Typography>
                    <Typography>Plaćeno: {kotizacija.placeno} RSD</Typography>
                    <Typography>Otpisano: {kotizacija.otpis} RSD</Typography>
                </div>
            );
        } else if (item.type === 5) {
            const response = await axios.get(`/api/racuns/${item.id}`);
            const racun = response.data;
            setModalContent(
                <div>
                    <Typography>Broj računa: {racun.invoiceNumber}</Typography>
                    <Typography>Datum: {new Date(racun.date).toLocaleDateString('sr-RS')}</Typography>
                    <Typography>Tip: Prodavnica</Typography>
                    <Typography>Ukupan iznos: {racun.totalAmount} RSD</Typography>
                    <Typography>Plaćeno: {racun.placeno === 0 ? racun.pretplata : racun.placeno} RSD</Typography>
                    <Typography>Otpisano: {racun.otpis} RSD</Typography>
                    <Typography>Stavke:</Typography>
                    <ul>
                        {racun.racunitems.map((item, index) => (
                            <li key={index}>
                                Naziv: {item.name}<br />
                                Količina: {item.quantity}<br />
                                Jedinična cena: {item.unitPrice} RSD<br />
                                Ukupno: {item.totalAmount} RSD
                            </li>
                        ))}
                    </ul>
                </div>
            );
        } else if (item.type === 6) {
            const response = await axios.get(`/api/uplatas/${item.id}`);
            const pozajmica = response.data;
            setModalContent(
                <div>
                    <Typography>Pozajmica: {pozajmica.totalAmount} RSD</Typography>
                    <Typography>Razduženo: {pozajmica.razduzeno} RSD</Typography>
                    <Typography>Datum: {new Date(pozajmica.date).toLocaleDateString('sr-RS')}</Typography>
                    {/*<Typography>Opis: {pozajmica.description}</Typography>*/}
                </div>
            );
        } else if (item.type === 7) {
            const response = await axios.get(`/api/zaduzenjes/${item.id}`);
            const otpisPozajmice = response.data;
            setModalContent(
                <div>
                    <Typography>Razduženo pozajmice: {otpisPozajmice.totalAmount} RSD</Typography>
                    <Typography>Datum: {new Date(otpisPozajmice.date).toLocaleDateString('sr-RS')}</Typography>
                    <Typography>Opis: {otpisPozajmice.opis}</Typography>
                </div>
            );
        } else if (item.type === 8) {
            const response = await axios.get(`/api/zaduzenjes/${item.id}`);
            const clanarina = response.data;
            setModalContent(
                <div>
                    <Typography>Ukupan iznos: {clanarina.totalAmount} RSD</Typography>
                    <Typography>Plaćeno: {clanarina.placeno} RSD</Typography>
                    <Typography>Otpisano: {clanarina.otpis} RSD</Typography>
                    <Typography>Datum: {new Date(clanarina.date).toLocaleDateString('sr-RS')}</Typography>
                </div>
            );
        } else if (item.type === 9) {
            const response = await axios.get(`/api/uplatas/${item.id}`);
            const otkazTermina = response.data;
            setModalContent(
                <div>
                    <Typography>Ukupan iznos: {otkazTermina.totalAmount} RSD</Typography>
                    <Typography>Razduženo: {otkazTermina.razduzeno} RSD</Typography>
                    <Typography>Datum: {new Date(otkazTermina.date).toLocaleDateString('sr-RS')}</Typography>
                    {/*<Typography>Opis: {pozajmica.description}</Typography>*/}
                </div>
            );
        }
        setOpenModal(true);
    };

    const getColor = (item) => {
        if (item.type === 2 ||
            item.type === 3 ||
            item.type === 6 ||
            item.type === 9) {
            return 'green';
        }

        const currentDate = new Date();
        const itemDate = new Date(item.date);
        const daysDifference = (currentDate - itemDate) / (1000 * 60 * 60 * 24);

        if (item.zaduzenje === item.razduzenje ||
            item.zaduzenje === item.pretplata) {
            return 'green';
        } else {
            return 'red';
        }
    };

    const getTypeLabel = (type) => {
        switch (type) {
            case 0:
                return 'Kafić';
            case 1:
                return 'Termin';
            case 2:
                return 'Uplata';
            case 3:
                return 'Otpis';
            case 4:
                return 'Kotizacije';
            case 5:
                return 'Prodavnica';
            case 6:
                return 'Pozajmica';
            case 7:
                return 'Otpis pozajmice';
            case 8:
                return 'Članarina';
            case 9:
                return 'Otkazan termin';
            default:
                return 'Nepoznato';
        }
    };

    const saldo = financialData.totalRazduzenje - financialData.totalZaduzenje;

    return (
        <Container component="main" maxWidth="lg" sx={{ mt: 8, mb: 4 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center' }}>
                <Typography component="h1" variant="h4" color="primary" gutterBottom>
                    Finansijska Kartica
                </Typography>
                <Typography variant="h6" gutterBottom>
                    Ukupno zaduženje: {financialData.totalZaduzenje} RSD
                </Typography>
                <Typography variant="h6" gutterBottom>
                    Ukupno razduženje: {financialData.totalRazduzenje} RSD
                </Typography>
                <Typography variant="h6" gutterBottom sx={{ color: saldo < 0 ? 'red' : saldo > 0 ? 'green' : 'inherit' }}>
                    SALDO: {saldo} RSD
                </Typography>

                <FormControl fullWidth variant="outlined" sx={{ mb: 3 }}>
                    <InputLabel id="filter-label" sx={{ color: '#000000' }}>Filtriraj Tip</InputLabel>
                    <Select
                        labelId="filter-label"
                        value={filterType}
                        onChange={(e) => setFilterType(e.target.value)}
                        label="Filtriraj Tip"
                    >
                        <MenuItem value="-1">Svi Tipovi</MenuItem>
                        <MenuItem value="0">Kafić</MenuItem>
                        <MenuItem value="1">Termin</MenuItem>
                        <MenuItem value="2">Uplata</MenuItem>
                        <MenuItem value="3">Otpis</MenuItem>
                        <MenuItem value="4">Kotizacije</MenuItem>
                        <MenuItem value="5">Prodavnica</MenuItem>
                        <MenuItem value="6">Pozajmica</MenuItem>
                        <MenuItem value="7">Otpis pozajmice</MenuItem>
                        <MenuItem value="8">Članarina</MenuItem>
                        <MenuItem value="9">Otkazan termin</MenuItem>
                    </Select>
                </FormControl>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 3 }}>
                    <TextField
                        label="Od datuma"
                        type="date"
                        value={fromDate}
                        onChange={(e) => setFromDate(e.target.value)}
                        InputLabelProps={{
                            shrink: true,
                            style: { color: 'black' }, // Dodato za crna slova
                        }}
                        sx={{ marginRight: 2 }}
                    />
                    <TextField
                        label="Do datuma"
                        type="date"
                        value={toDate}
                        onChange={(e) => setToDate(e.target.value)}
                        InputLabelProps={{
                            shrink: true,
                            style: { color: 'black' }, // Dodato za crna slova
                        }}
                    />
                </Box>
                <Box sx={{ mt: 3, width: '100%', display: 'flex', flexDirection: 'column', gap: 2 }}>
                    {financialData.items.map((item, index) => (
                        <Card key={index} sx={{ backgroundColor: getColor(item), cursor: 'pointer' }} onClick={() => handleCardClick(item)}>
                            <CardContent>
                                <Typography variant="h6">{new Date(item.date).toLocaleDateString('sr-RS')}</Typography>
                                <Typography>{`Tip: ${getTypeLabel(item.type)}`}</Typography>
                                {item.type !== 2 && item.type !== 3 && item.type !== 6 && item.type !== 9 && (
                                    <>
                                        <Typography>{`Zaduženje: ${item.zaduzenje} RSD`}</Typography>
                                        <Typography>{`Otpis: ${item.otpis} RSD`}</Typography>
                                    </>
                                )}
                                {item.type === 2 || item.type === 3 || item.type === 6 ? (
                                    <Typography>{`Razduženje: ${item.totalAmount} RSD`}</Typography>
                                ) : (
                                    <Typography>{`Razduženje: ${item.razduzenje === 0 ? item.pretplata : item.razduzenje} RSD`}</Typography>
                                )}
                                
                            </CardContent>
                        </Card>
                    ))}
                </Box>
                <Pagination
                    count={totalPages}
                    page={page}
                    onChange={(event, value) => setPage(value)}
                    color="primary"
                    sx={{ mt: 3 }}
                    hideNextButton={page >= totalPages}
                    hidePrevButton={page <= 1}
                />
            </Box>
            <Modal
                open={openModal}
                onClose={() => setOpenModal(false)}
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
                        Detalji
                    </Typography>
                    <Typography id="modal-modal-description" sx={{ mt: 2 }}>
                        {modalContent}
                    </Typography>
                    <Button
                        type="button"
                        fullWidth
                        variant="outlined"
                        color="secondary"
                        sx={{ mt: 2 }}
                        onClick={() => setOpenModal(false)}
                    >
                        Zatvori
                    </Button>
                </Box>
            </Modal>
        </Container>
    );
};

export default FinancialCard;