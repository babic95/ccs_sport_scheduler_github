import React, { useState, useEffect, useRef } from 'react';
import FullCalendar from '@fullcalendar/react';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin from '@fullcalendar/interaction';
import axios from 'axios';
import { Modal, Button, Typography, Box, Container, FormControl, InputLabel, Select, MenuItem, CircularProgress } from '@mui/material';
import { styled } from '@mui/system';
import 'bootstrap/dist/css/bootstrap.min.css';
import './Schedule.css'; // Uverite se da je CSS datoteka pravilno importovana
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
    const [terenId, setTerenId] = useState('');
    const [tereni, setTereni] = useState([]);
    const [termini, setTermini] = useState([]);
    const [selectedEvent, setSelectedEvent] = useState(null);
    const [showModal, setShowModal] = useState(false);
    const [cenaTermina, setCenaTermina] = useState([]);
    const [selectedDate, setSelectedDate] = useState(new Date());
    const [notifications, setNotifications] = useState([]);
    const [unreadCount, setUnreadCount] = useState(0);
    const [selectedNotification, setSelectedNotification] = useState(null);
    const [users, setUsers] = useState([]);
    const [selectedUser, setSelectedUser] = useState(user.id);
    const [loading, setLoading] = useState(true);
    const { startOfWeek, endOfWeek } = getWeekRange();

    const [financialData, setFinancialData] = useState({
        totalZaduzenje: 0,
        totalRazduzenje: 0,
        items: [],
        totalCount: 0
    });

    const handleWindowResize = () => {
        if (calendarRef.current && calendarRef.current.el) {
            const calendarEl = calendarRef.current.el;
            const containerWidth = calendarEl.parentNode.offsetWidth;

            // Dodajte marginu kontejnera
            const containerMargin = 32; // Primer margine u pikselima, prilagodite prema potrebi
            const containerWidthWithMargin = containerWidth - containerMargin;

            // Formula for scaling
            const columnWidth = 18.5;
            const daysToShow = 7;
            const desiredWidth = columnWidth * daysToShow;
            const scale = containerWidthWithMargin / desiredWidth;

            calendarEl.style.transform = `scale(${scale})`;
            calendarEl.style.transformOrigin = '0 0';
        }
    };

    useEffect(() => {
        if (calendarRef.current) {
            handleWindowResize();
            window.addEventListener('resize', handleWindowResize);
        }
        return () => {
            window.removeEventListener('resize', handleWindowResize);
        };
    }, [calendarRef]);

    useEffect(() => {
        const fetchTereni = async () => {
            try {
                const response = await axios.get('/api/Klubs/teren/1');
                console.log(response.data);
                setTereni(response.data);
                if (response.data.length > 0) {
                    setTerenId(response.data[0].id);  // Postavite prvi element kao inicijalni teren
                }
            } catch (error) {
                console.error('Error fetching tereni:', error);
            }
        };

        const fetchUsers = async () => {
            try {
                const response = await axios.get('/api/users/getAllUsersFromKlub/1');
                console.log(response.data);
                setUsers(response.data);
            } catch (error) {
                console.error('Error fetching users:', error);
            }
        };

        const fetchData = async () => {
            setLoading(true);
            await fetchTereni();
            await fetchUsers();
        };

        fetchData();


    }, []);

    const fetchTermini = async (startDate, endDate) => {
        try {
            setLoading(true);
            const response = await axios.get(`/api/termins/zakazaniTermini/${terenId}/${selectedUser}`);
            console.log(response.data);

            const zakazaniTermini = response.data.map((termin, i) => ({
                id: termin.id ? termin.id : `slobodan-${cenaTermina.id}-${i}-${new Date(termin.startDateTime).getHours()}`,
                title: termin.user ? `${termin.user.username}` : `${termin.price}`,
                start: new Date(termin.startDateTime).toISOString(),
                end: new Date(termin.endDateTime).toISOString(),
                extendedProps: {
                    user: termin.user,
                    price: termin.price,
                    type: termin.user ? termin.user.type : null
                },
                className: termin.user ? eventClassNames({ event: { extendedProps: { type: termin.user.type } } }) : '',
                backgroundColor: termin.type ? eventBackgoundCollor({ event: { extendedProps: { type: termin.type } } }) : 'white', // Postavljanje bele boje za slobodne termine
                textColor: 'black',
                borderColor: 'black',
            }));

            setTermini([]);
            console.log(zakazaniTermini);
            setTermini(zakazaniTermini);

        } catch (error) {
            console.error('Error fetching termini:', error);
        }
        setLoading(false);
    };

    const generateSlobodniTermini = (startDate, endDate, cenaTermina) => {
        const slobodniTermini = [];
        for (let i = 0; i < 7; i++) {
            const currentDate = new Date(startDate);
            currentDate.setDate(startDate.getDate() + i);

            for (let hour = 7; hour < 23; hour++) {
                const startTime = new Date(currentDate);
                startTime.setHours(hour, 0, 0, 0);
                const endTime = new Date(startTime);
                endTime.setHours(hour + 1, 0, 0, 0);

                if (!isNaN(startTime.getTime()) && !isNaN(endTime.getTime())) {
                    slobodniTermini.push({
                        id: `slobodan-${cenaTermina.id}-${i}-${hour}`,
                        title: `${cenaTermina.price}`,
                        start: startTime.toISOString(),
                        end: endTime.toISOString(),
                        extendedProps: {
                            price: cenaTermina.price,
                        },
                        backgroundColor: 'white',
                        textColor: 'black',
                        borderColor: 'black',
                    });
                } else {
                    console.error('Invalid time value for slobodni termini:', startTime, endTime);
                }
            }
        }
        return slobodniTermini;
    };

    const fetchFinancialData = async () => {
        try {
            const response = await axios.get(`/api/users/financialCard/${selectedUser}`);
            setFinancialData(response.data);
        } catch (error) {
            console.error('Error fetching financial data:', error);
        }
    };

    useEffect(() => {
        if (terenId !== '') {
            fetchTermini(startOfWeek, endOfWeek);
        }
        fetchUnreadNotifications();
        fetchFinancialData();
    }, [terenId, selectedUser]);

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
        const startTime = new Date(info.dateStr);
        const endTime = new Date(startTime);
        endTime.setHours(startTime.getHours() + 1);

        const today = new Date();
        const nextMonday = new Date(today);
        nextMonday.setDate(today.getDate() + 7);

        if (startTime < today || startTime > nextMonday) {
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
            id: null,
            start: startTime,
            end: endTime,
            price: price,
            user: null
        });

        setShowModal(true);
    };

    const handleEventClick = (clickInfo) => {
        console.log(clickInfo.event.extendedProps);
        setSelectedEvent({
            id: clickInfo.event.id,
            start: new Date(clickInfo.event.startStr),
            end: new Date(clickInfo.event.endStr),
            user: clickInfo.event.extendedProps.user,
            price: clickInfo.event.extendedProps.price
        });
        setShowModal(true);
    };
    
    const handleOtkazi = async () => {
        if (selectedEvent) {
            try {
                
                await axios.delete(`/api/termins/${user.id}/${selectedEvent.id}`);
                alert('Termin uspešno otkazan!');

                setTermini([]);
                fetchTermini(startOfWeek, endOfWeek);

                setSelectedEvent(null);
                setShowModal(false);
            } catch (error) {
                console.error('Error making otkazivanje:', error);
                alert('Greška prilikom otkazivanja: ' + (error.response?.data?.message || error.message));
            }
        }
    };
    const handleReservation = async () => {
        if (selectedEvent) {
            try {
                const reservationData = {
                    TerenId: terenId,
                    UserId: selectedUser,
                    StartDateTime: selectedEvent.start.toISOString(),
                    EndDateTime: selectedEvent.end.toISOString()
                };

                await axios.post(`/api/termins/zakazi`, reservationData);
                alert('Termin rezervisan uspešno!');

                setTermini([]);
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

        startOfWeek.setDate(today.getDate());
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

    const eventClassNames = (eventInfo) => {
        switch (eventInfo.event.extendedProps.type) {
            case 0:
                return 'event-fiksni'; // Fiksni
            case 1:
                return 'event-plivajuci'; // Plivajući
            case 2:
                return 'event-trenerski'; // Trenerski
            case 3:
                return 'event-vanredni'; // Vanredni
            case 4:
                return 'event-neclanski'; // Neclanski
            case 5:
                return 'event-klupski'; // Klupski
            default:
                return '';
        }
    };

    const eventBackgoundCollor = (eventInfo) => {
        switch (eventInfo.event.extendedProps.type) {
            case 0:
                return 'gray'; // Fiksni
            case 1:
                return 'yellow'; // Plivajući
            case 2:
                return 'green'; // Trenerski
            case 3:
                return 'red'; // Vanredni
            case 4:
                return 'blue'; // Neclanski
            case 5:
                return 'brown'; // Klupski
            default:
                return '';
        }
    };

    const Legend = () => (
        <Box className="legend-container">
            <div className="legend-item">
                <div className="legend-color event-fiksni"></div> Fiksni
            </div>
            <div className="legend-item">
                <div className="legend-color event-plivajuci"></div> Plivajući
            </div>
            <div className="legend-item">
                <div className="legend-color event-trenerski"></div> Trenerski
            </div>
            <div className="legend-item">
                <div className="legend-color event-vanredni"></div> Vanredni
            </div>
            <div className="legend-item">
                <div className="legend-color event-neclanski"></div> Neclanski
            </div>
            <div className="legend-item">
                <div className="legend-color event-klupski"></div> Klupski
            </div>
        </Box>
    );

    const saldo = financialData.totalRazduzenje - financialData.totalRazduzenje;

    return (
        <Container component="main" maxWidth="lg" sx={{ mt: 8, mb: 4 }}>
            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                    <CircularProgress />
                </Box>
            ) : (
                <>
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
                            {tereni.map((teren) => (
                                <MenuItem key={teren.id} value={teren.id}>
                                    {teren.name}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>

                    {(user.type === 9 || user.type === 8) && (
                        <FormControl fullWidth variant="outlined" sx={{ mb: 3 }}>
                            <InputLabel id="user-label" sx={{ color: '#000000' }}>Izaberi Korisnika</InputLabel>
                            <Select
                                labelId="user-label"
                                value={selectedUser}
                                onChange={(e) => setSelectedUser(e.target.value)}
                                label="Izaberi Korisnika"
                                sx={{ color: '#000000' }}
                            >
                                {users.map((user) => (
                                    <MenuItem key={user.id} value={user.id}>
                                        {user.username}
                                    </MenuItem>
                                ))}
                            </Select>
                        </FormControl>
                    )}

                    <div className="centerContainer">
                        <Legend />
                    </div>
                    <div className="zoom-container">
                        <div className="zoom-content" ref={calendarRef}>
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
                                eventClassNames={eventClassNames} // Dodato za prilagođene klase
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

                    {notifications.length !== 0 ? (
                        <Notifications
                            notifications={notifications}
                            handleNotificationClick={handleNotificationClick}
                            style={{ marginTop: '10px' }}
                        />
                    ) : null}

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
                                            {user.id === selectedEvent.user.id || user.type === 9 || user.type === 8 ?
                                                <Box sx={{ mt: 2, display: 'flex', justifyContent: 'space-between' }}>
                                                    <Button variant="contained" color="secondary" onClick={() => setShowModal(false)}>
                                                        Zatvori
                                                    </Button>
                                                    <Button variant="contained" color="primary" onClick={handleOtkazi}>
                                                        Otkaži
                                                    </Button>
                                                </Box> : 
                                                <Button variant="contained" color="primary" onClick={() => setShowModal(false)} sx={{ display: 'block', margin: '20px auto' }}>
                                                    Zatvori
                                                </Button>
                                            }
                                        
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
                </>
            )}
        </Container>
    );
};

export default Schedule;
