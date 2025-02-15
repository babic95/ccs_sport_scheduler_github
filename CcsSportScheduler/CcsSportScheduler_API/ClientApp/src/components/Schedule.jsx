import React, { useState, useEffect, useRef } from 'react';
import FullCalendar from '@fullcalendar/react';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import axios from 'axios';
import { Modal, Button, Typography, Box, Container, FormControl, InputLabel, Select, MenuItem } from '@mui/material';
import { styled } from '@mui/system';
import 'bootstrap/dist/css/bootstrap.min.css';
import './Schedule.css';
import srLocale from '@fullcalendar/core/locales/sr'; // Import Serbian locale
import Notifications from './Notifications'; // Import Notification component

const CustomModal = styled(Modal)({
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
});

const CustomBox = styled(Box)({
    backgroundColor: '#fff',
    padding: '20px',
    borderRadius: '8px',
    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
});

const Schedule = ({ user }) => {
    const calendarRef = useRef(null);
    const [terenId, setTerenId] = useState(1);
    const [termini, setTermini] = useState([]);
    const [selectedEvent, setSelectedEvent] = useState(null);
    const [showModal, setShowModal] = useState(false);
    const [cenaTermina, setCenaTermina] = useState([]);
    const [selectedDate, setSelectedDate] = useState(new Date());
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [selectedNotification, setSelectedNotification] = useState(null);
    const { startOfWeek, endOfWeek } = getWeekRange();

    const [financialData, setFinancialData] = useState({
        totalZaduzenje: 0,
        totalRazduzenje: 0,
        items: [],
        totalCount: 0
    });

    const fetchTermini = async (startDate, endDate) => {
        try {
            const response = await axios.get(`/api/termins/zakazaniTermini/${terenId}`, {
                params: {
                    startDate: startDate.toISOString(),
                    endDate: endDate.toISOString()
                }
            });

            console.log('Termini:', response.data);

            const zakazaniTermini = response.data.map(termin => ({
                id: termin.id,
                title: termin.user ? `${termin.user.username}` : `${termin.price}`,
                start: new Date(termin.startDateTime).toISOString(),
                end: new Date(termin.endDateTime).toISOString(),
                extendedProps: {
                    user: termin.user,
                    price: termin.price
                },
                backgroundColor: termin.user ? 'lightcoral' : 'white',
                textColor: 'black',
                borderColor: 'black',
            }));

            setTermini([]);

            const responseCenaTermina = await axios.get(`/api/klubs/naplataTermina/${terenId}`);
            const slobodniTermini = generateSlobodniTermini(startDate, endDate, responseCenaTermina.data);

            setTermini([...zakazaniTermini, ...slobodniTermini]);
        } catch (error) {
            console.error('Error fetching termini:', error);
        }
    };

    const generateSlobodniTermini = (startDate, endDate, cenaTermina) => {
        const slobodniTermini = [];
        for (let i = 0; i < 7; i++) {
            const currentDate = new Date(startDate);
            currentDate.setDate(startDate.getDate() + i);

            cenaTermina.forEach(cena => {
                for (let hour = cena.startTime; hour < cena.endTime; hour++) {
                    const startTime = new Date(currentDate);
                    startTime.setHours(hour, 0, 0, 0);
                    const endTime = new Date(startTime);
                    endTime.setHours(hour + 1, 0, 0, 0);

                    if (!isNaN(startTime.getTime()) && !isNaN(endTime.getTime())) {
                        slobodniTermini.push({
                            id: `slobodan-${cena.id}-${i}-${hour}`,
                            title: `${cena.price}`,
                            start: startTime.toISOString(),
                            end: endTime.toISOString(),
                            extendedProps: {
                                price: cena.price,
                            },
                            backgroundColor: 'white',
                            textColor: 'black',
                            borderColor: 'black',
                        });
                    } else {
                        console.error('Invalid time value for slobodni termini:', startTime, endTime);
                    }
                }
            });
        }
        return slobodniTermini;
    };

    const fetchFinancialData = async () => {
        try {
            const response = await axios.get(`/api/users/financialCard/${user.id}`);
            setFinancialData(response.data);
        } catch (error) {
            console.error('Error fetching financial data:', error);
        }
    };

    useEffect(() => {
        fetchTermini(startOfWeek, endOfWeek);
        fetchUnreadNotifications();
        fetchFinancialData();
    }, [terenId]);

    const fetchUnreadNotifications = async () => {
        try {
            const response = await axios.get('/api/obavestenjas/nepregledan/1');
            setUnreadCount(response.data ? response.data.length : 0);
            setNotifications(response.data || []);
        } catch (error) {
            console.error('Error fetching notifications:', error);
        }
    };

    const handleDateClick = (info) => {
        console.log('Date clicked:', info);
        const startTime = new Date(info.dateStr);
        const endTime = new Date(startTime);
        endTime.setHours(startTime.getHours() + 1);

        const today = new Date();
        const nextMonday = new Date(today);
        nextMonday.setDate(today.getDate() + 7);

        if (startTime < today || startTime > nextMonday) {
            console.log('Invalid date selected. Only current week is selectable.');
            alert('Možete rezervisati termine samo u tekućoj nedelji.');
            return;
        }

        let day = startTime.getDay();
        let isWeekend = (day === 6 || day === 0);

        let price = 300;
        let termin = cenaTermina.find(cena => cena.startTime <= startTime.getHours() && cena.endTime > startTime.getHours() && isWeekend === cena.vikend);

        if (termin) {
            price = termin.price;
        }

        setSelectedEvent({
            start: startTime,
            end: endTime,
            price: price,
            user: null
        });

        setShowModal(true);
    };

    const handleEventClick = (clickInfo) => {
        console.log('Event clicked:', clickInfo);
        setSelectedEvent({
            start: new Date(clickInfo.event.startStr),
            end: new Date(clickInfo.event.endStr),
            user: clickInfo.event.extendedProps.user,
            price: clickInfo.event.extendedProps.price
        });
        setShowModal(true);
    };

    const handleReservation = async () => {
        console.log('Reservation data:', selectedEvent);
        if (selectedEvent) {
            try {
                const reservationData = {
                    TerenId: terenId,
                    UserId: user.id,
                    StartDateTime: selectedEvent.start.toISOString(),
                    EndDateTime: selectedEvent.end.toISOString()
                };

                await axios.post(`/api/termins/zakazi`, reservationData);
                alert('Termin rezervisan uspešno!');

                setTermini([]);
                // Osvežavanje liste termina nakon uspešne rezervacije
                fetchTermini(startOfWeek, endOfWeek);

                setSelectedEvent(null);
                setShowModal(false);
            } catch (error) {
                console.error('Error making reservation:', error);
                alert('Greška prilikom rezervacije: ' + error.response.data.message);
            }
        }
    };

    function getWeekRange() {
        const today = new Date();
        const startOfWeek = new Date(today);
        const endOfWeek = new Date(today);

        // Start of the week is today
        startOfWeek.setDate(today.getDate());

        // End of the week is next Monday
        endOfWeek.setDate(today.getDate() + 7);

        return { startOfWeek, endOfWeek };
    }

    const handleNotificationClick = async (notification) => {
        setSelectedNotification(notification);
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

    const renderEventContent = (eventInfo) => {
        return (
            <div>
                <b>{eventInfo.event.extendedProps.user ? eventInfo.event.extendedProps.user.username : `${eventInfo.event.extendedProps.price}`}</b>
            </div>
        );
    };

    const formatDateTime = (date) => {
        return new Date(date).toLocaleString('sr-RS', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    const saldo = financialData.totalRazduzenje - financialData.totalZaduzenje;

    return (
        <Container component="main" maxWidth="lg" sx={{ mt: 8, mb: 4 }}>

            <Typography variant="h6" align="center" gutterBottom>
                Ukupno zaduženje: {financialData.totalZaduzenje} RSD
            </Typography>
            <Typography variant="h6" align="center" gutterBottom>
                Ukupno razduženje: {financialData.totalRazduzenje} RSD
            </Typography>
            <Typography variant="h6" align="center" gutterBottom sx={{ color: saldo < 0 ? 'red' : saldo > 0 ? 'green' : 'inherit' }}>
                SALDO: {saldo} RSD
            </Typography>

            <Typography component="h1" variant="h4" color="primary" align="center" gutterBottom>
                Rezervacija Termina
            </Typography>

            <FormControl fullWidth variant="outlined" sx={{ mb: 3 }}>
                <InputLabel id="teren-label" sx={{ color: '#000000' }}>Izaberi Teren</InputLabel>
                <Select
                    labelId="teren-label"
                    value={terenId}
                    onChange={(e) => setTerenId(e.target.value)}
                    label="Izaberi Teren"
                    sx={{ color: '#000000' }}
                >
                    <MenuItem value={1}>Teren 1</MenuItem>
                    <MenuItem value={2}>Teren 2</MenuItem>
                </Select>
            </FormControl>

            <div className="zoom-container" style={{ overflowX: 'auto' }}>
                <div ref={calendarRef} style={{ width: '100%' }}>
                    <FullCalendar
                        plugins={[timeGridPlugin, interactionPlugin]}
                        initialView="timeGridWeek"
                        headerToolbar={{
                            left: '',
                            center: 'title',
                            right: ''
                        }}
                        locale='sr' // Serbian locale
                        allDaySlot={false}
                        slotMinTime="07:00:00"
                        slotMaxTime="23:00:00"
                        slotDuration="01:00:00" // Interval je sada sat vremena
                        events={termini}
                        dateClick={handleDateClick}
                        eventClick={handleEventClick}
                        initialDate={selectedDate} // Postavljanje početnog datuma na današnji dan
                        validRange={{ start: startOfWeek, end: endOfWeek }}
                        views={{
                            timeGridWeek: {
                                type: 'timeGridWeek',
                                duration: { weeks: 1 },
                                buttonText: 'Nedelja'
                            },
                            timeGridDay: {
                                type: 'timeGridDay',
                                duration: { days: 1 },
                                buttonText: 'Dan'
                            }
                        }}
                        firstDay={selectedDate.getDay()} // Postavljanje prvog dana na trenutni dan
                        handleWindowResize={true}
                        windowResizeDelay={100}
                        longPressDelay={0}
                        selectLongPressDelay={0}
                        eventLongPressDelay={0}
                        timeZone='local' // Postavljanje vremenske zone na lokalnu
                        eventContent={renderEventContent} // Koristi prilagođeni sadržaj događaja
                    />
                </div>
            </div>

            {notifications.length !== 0 ? (<Notifications notifications={notifications} handleNotificationClick={handleNotificationClick} />) : null}

            {selectedNotification && (
                <CustomModal
                    open={Boolean(selectedNotification)}
                    onClose={handleModalClose}
                    aria-labelledby="notification-modal-title"
                    aria-describedby="notification-modal-description"
                >
                    <CustomBox>
                        <Typography variant="h6" id="notification-modal-title">Obaveštenje</Typography>
                        <Typography id="notification-modal-description">{selectedNotification.description}</Typography>
                        <Button variant="contained" color="primary" onClick={handleModalClose} sx={{ display: 'block', margin: '20px auto' }}>Zatvori</Button>
                    </CustomBox>
                </CustomModal>
            )}

            {showModal && (
                <CustomModal
                    open={showModal}
                    onClose={() => setShowModal(false)}
                    aria-labelledby="reservation-modal-title"
                    aria-describedby="reservation-modal-description"
                >
                    <CustomBox>
                        {selectedEvent && selectedEvent.user ? (
                            <>
                                <Typography variant="h6" id="reservation-modal-title">Pregled Termina</Typography>
                                <Typography variant="body1">Rezervisano od: {selectedEvent.user.username}</Typography>
                                <Typography variant="body1">Početak: {formatDateTime(selectedEvent.start)}</Typography>
                                <Typography variant="body1">Kraj: {formatDateTime(selectedEvent.end)}</Typography>
                                <Button variant="contained" color="primary" onClick={() => setShowModal(false)} sx={{ display: 'block', margin: '20px auto' }}>
                                    Zatvori
                                </Button>
                            </>
                        ) : (
                            <>
                                <Typography variant="h6" id="reservation-modal-title">Zakazivanje Termina</Typography>
                                <Typography variant="body1">Početak: {formatDateTime(selectedEvent.start)}</Typography>
                                <Typography variant="body1">Kraj: {formatDateTime(selectedEvent.end)}</Typography>
                                <Typography variant="body1">Cena: {selectedEvent.price} RSD</Typography>
                                <Box sx={{ mt: 2, display: 'flex', justifyContent: 'space-between' }}>
                                    <Button variant="contained" color="secondary" onClick={() => setShowModal(false)}>
                                        Zatvori
                                    </Button>
                                    <Button variant="contained" color="primary" onClick={handleReservation}>
                                        Rezerviši
                                    </Button>
                                </Box>
                            </>
                        )}
                    </CustomBox>
                </CustomModal>
            )}
        </Container>
    );
};

export default Schedule;