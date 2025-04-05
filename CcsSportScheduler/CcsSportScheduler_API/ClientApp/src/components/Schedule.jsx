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
//import { createPlugin, locale as coreLocale } from '@fullcalendar/core';

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

const customLocale = {
    code: "sr",
    week: {
        dow: 1, // Prvi dan u sedmici (ponedeljak)
        doy: 7, // Prva sedmica u godini koja ima barem 7 dana
    },
    buttonText: {
        prev: "Prethodna",
        next: "Sledeća",
        today: "Danas",
        month: "Mesec",
        week: "Sedmica",
        day: "Dan",
        list: "Planer",
    },
    weekText: "Sed",
    allDayText: "Ceo dan",
    moreLinkText: function (n) {
        return "+ još " + n;
    },
    noEventsText: "Nema događaja za prikaz",
};

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
            const containerHeight = calendarEl.parentNode.offsetHeight;

            // Dodajte marginu kontejnera
            const containerMargin = 32; // Primer margine u pikselima, prilagodite prema potrebi
            const containerWidthWithMargin = containerWidth - containerMargin;
            const containerHeightWithMargin = containerHeight - containerMargin;

            // Formula for scaling
            const columnWidth = 22; // Minimalna širina kolone je sada 22 karaktera
            const daysToShow = 7;
            const desiredWidth = columnWidth * daysToShow;
            const scale = containerWidthWithMargin / desiredWidth;

            calendarEl.style.transform = `scale(${scale})`;
            calendarEl.style.transformOrigin = 'top left';
        }
    };
    const zoomIn = () => {
        if (calendarRef.current && calendarRef.current.el) {
            const calendarEl = calendarRef.current.el;
            const currentScale = parseFloat(calendarEl.style.transform.replace(/[^0-9.-]/g, '')) || 1;
            const newScale = currentScale + 0.1;
            calendarEl.style.transform = `scale(${newScale})`;
            calendarEl.style.transformOrigin = 'top left';
        }
    };

    const zoomOut = () => {
        if (calendarRef.current && calendarRef.current.el) {
            const calendarEl = calendarRef.current.el;
            const currentScale = parseFloat(calendarEl.style.transform.replace(/[^0-9.-]/g, '')) || 1;
            const newScale = currentScale - 0.1;
            calendarEl.style.transform = `scale(${newScale})`;
            calendarEl.style.transformOrigin = 'top left';
        }
    };
    const logZoomContainerWidth = () => {
        const zoomContainer = document.querySelector('.zoom-container');
        if (zoomContainer) {
            console.log('Zoom container width:', zoomContainer.offsetWidth);
        }
    };
    useEffect(() => {
        logZoomContainerWidth(); // Ispiši širinu kada se komponenta učita

        window.addEventListener('resize', logZoomContainerWidth);

        return () => {
            window.removeEventListener('resize', logZoomContainerWidth);
        };
    }, []);

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

    const fetchTermini = async (startDate) => {
        try {
            setLoading(true);
            // Formatiraj startDate u ISO string bez vremenske zone
            const startDateString = startDate.toISOString().split('T')[0];
            // Dodaj startDate kao query parametar
            const response = await axios.get(`/api/termins/zakazaniTermini/${terenId}/${selectedUser}?startDate=${startDateString}`);
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
                backgroundColor: termin.type >= 0 ? eventBackgoundCollor({ event: { extendedProps: { type: termin.type } } }) : 'white', // Postavljanje bele boje za slobodne termine
                textColor: 'black',
                borderColor: 'black',
            }));

            setTermini(zakazaniTermini);

        } catch (error) {
            console.error('Error fetching termini:', error);
        }
        setLoading(false);
    };

    // Unutar komponente ili useEffect-a
    const previousStartDate = useRef(null);

    const handleDatesSet = (arg) => {
        const { startOfWeek, endOfWeek } = getWeekRange(arg.start);

        var startOfWeek7h = new Date(startOfWeek.setHours(7, 0, 0, 0));

        console.log("startOfWeek7h");
        console.log(startOfWeek7h);
        const oldStartDate = previousStartDate.current ? new Date(previousStartDate.current) : null;

        console.log("oldStartDate");
        console.log(oldStartDate);
        if (!oldStartDate || startOfWeek7h.getDate() !== oldStartDate.getDate()) {
            fetchTermini(startOfWeek7h);
            previousStartDate.current = startOfWeek7h;
            setSelectedDate(startOfWeek7h);
        }
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

    const previousTerenId = useRef(terenId);
    const previousSelectedUser = useRef(selectedUser);

    useEffect(() => {
        const today = new Date();
        today.setHours(7, 0, 0, 0);
        if (terenId !== '' && (previousTerenId.current !== terenId || previousSelectedUser.current !== selectedUser)) {
            previousTerenId.current = terenId;
            previousSelectedUser.current = selectedUser;
            fetchTermini(today);  // Koristi današnji datum umesto startOfWeek
        }
        setSelectedDate(today);
        fetchUnreadNotifications();
        fetchFinancialData();
    }, [terenId, selectedUser]);

    const fetchUnreadNotifications = async () => {
        try {
            const response = await axios.get('/api/obavestenjas/nepregledan/' + user.id);
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
        today = today.setHours(7, 0, 0, 0);
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

    const convertToCEST = (date) => {
        // Create a new date object from the given date
        const utcDate = new Date(date.toLocaleString('en-US', { timeZone: 'UTC' }));
        // Convert the UTC date to CEST
        const cestDate = new Date(utcDate.toLocaleString('en-US', { timeZone: 'Europe/Belgrade' }));
        return cestDate;
    };

    const handleEventClick = (clickInfo) => {
        const eventStart = convertToCEST(new Date(clickInfo.event.startStr));
        const now = convertToCEST(new Date());

        console.log(clickInfo.event.extendedProps);
        if (clickInfo.event.extendedProps.user === null &&
            eventStart < now) {
            alert('Ne možete rezervisati termin koji je prošao.');
            return;
        }

        setSelectedEvent({
            id: clickInfo.event.id,
            start: new Date(clickInfo.event.startStr),
            end: new Date(clickInfo.event.endStr),
            user: clickInfo.event.extendedProps.user,
            price: clickInfo.event.extendedProps.price,
            type: clickInfo.event.extendedProps.type
        });
        setShowModal(true);
    }; 

    const handleOtkaziNoFree = async () => {
        if (selectedEvent) {
            try {

                await axios.delete(`/api/termins/${user.id}/${selectedEvent.id}?isFree=false`);
                alert('Termin uspešno otkazan!');

                setTermini([]);
                fetchTermini(selectedDate);

                setSelectedEvent(null);
                setShowModal(false);
            } catch (error) {
                console.error('Error making otkazivanje:', error);
                alert('Greška prilikom otkazivanja: ' + (error.response?.data?.message || error.message));
            }
        }
    };
    const handleOtkaziFiksni = async () => {
        if (selectedEvent) {
            try {
                const otkazivanjeData = {
                    TerenId: terenId,
                    UserId: selectedEvent.user.id,
                    Date: selectedEvent.start.toISOString()
                };

                await axios.post(`/api/termins/otkazi/fiksni`, otkazivanjeData);
                alert('Fiksni termini uspešno otkazani!');

                setTermini([]);
                fetchTermini(selectedDate);

                setSelectedEvent(null);
                setShowModal(false);
            } catch (error) {
                console.error('Error making otkazivanje fiksnog:', error);
                alert('Greška prilikom otkazivanja fiksnog: ' + (error.response?.data?.message || error.message));
            }
        }
    };
    const handleOtkazi = async () => {
        if (selectedEvent) {
            try {

                await axios.delete(`/api/termins/${user.id}/${selectedEvent.id}`);
                alert('Termin uspešno otkazan!');

                setTermini([]);
                fetchTermini(selectedDate);

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
                    EndDateTime: selectedEvent.end.toISOString(),
                    Zaduzi: 1,
                };

                await axios.post(`/api/termins/zakazi`, reservationData);
                alert('Termin rezervisan uspešno!');

                setTermini([]);
                fetchTermini(selectedDate);

                setSelectedEvent(null);
                setShowModal(false);
            } catch (error) {
                console.error('Error making reservation:', error);
                alert('Greška prilikom rezervacije: ' + error.response.data.message);
            }
        }
    };

    function getWeekRange(start) {
        const startOfWeek = new Date(start);
        const endOfWeek = new Date(startOfWeek);

        // Postavi kraj nedelje na nedelju
        endOfWeek.setDate(startOfWeek.getDate() + 6);

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

    // Definiši validRange na osnovu autorizacije korisnika
    const validRange = user.type === 9 || user.type === 8 ? {} : { start: startOfWeek, end: endOfWeek };

    // Definiši headerToolbar na osnovu autorizacije korisnika
    const headerToolbar = {
        left: '',
        center: 'title',
        right: ''
    };

    const footerToolbar = user.type === 9 || user.type === 8
        ? {
            left: 'prev,next',
            center: 'today',
            right: 'timeGridWeek,timeGridDay'
        }
        : {
            left: '',
            center: '',
            right: ''
        };
    function updateTitle(view) {
        const customMonthNames = [
            'Jan', 'Feb', 'Mar', 'Apr', 'Maj', 'Jun',
            'Jul', 'Avg', 'Sep', 'Okt', 'Novr', 'Dec'
        ];

        // Dobijamo trenutni mesec i godinu
        const currentMonth = customMonthNames[view.currentStart.getMonth()];
        const currentYear = view.currentStart.getFullYear();

        // Početni i krajnji datum prikaza
        const startDate = view.activeStart;
        const endDate = view.activeEnd;

        // Formatiramo datume u "dd.mm"
        const formatDate = (date) => {
            const day = date.getDate().toString().padStart(2, '0');
            const month = (date.getMonth() + 1).toString().padStart(2, '0');
            return `${day}.${month}`;
        };

        const formattedStartDate = formatDate(startDate);
        const formattedEndDate = formatDate(endDate);

        // Promeni naslov kalendara
        const titleElement = document.querySelector('.fc-toolbar-title');
        if (titleElement) {
            // Naslov u formatu: "12.03 - 18.03, Mart 2025"
            titleElement.innerText = `${formattedStartDate} - ${formattedEndDate} ${currentMonth} ${currentYear}`;
        }
    }



    //const headerToolbar = user.type === 9 || user.type === 8
    //    ? {
    //        left: '',
    //        center: 'title',
    //        right: ''
    //    }
    //    : {
    //        left: '',
    //        center: 'title',
    //        right: ''
    //    };

    const saldo = financialData.totalRazduzenje - financialData.totalZaduzenje;

    return (
        <Container component="main" maxWidth="lg" sx={{ mt: 8, mb: 4 }}>
            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh' }}>
                    <CircularProgress />
                </Box>
            ) : (
                <>
                        {
                            //<Typography variant="h6" align="center" gutterBottom>
                            //    Ukupno zaduženje: {financialData.totalZaduzenje} RSD
                            //</Typography>
                            //<Typography variant="h6" align="center" gutterBottom>
                            //    Ukupno razduženje: {financialData.totalRazduzenje} RSD
                            //</Typography>
                        }
                    <Typography variant="h6" align="center" gutterBottom sx={{ color: saldo < 0 ? 'red' : saldo > 0 ? 'green' : 'inherit' }}>
                        SALDO: {saldo} RSD
                    </Typography>

                        {
                            //<Typography component="h1" variant="h4" color="primary" align="center" gutterBottom>
                            //    Rezervacija Termina
                            //</Typography>
                        }

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
                                    headerToolbar={headerToolbar}
                                    footerToolbar={footerToolbar}
                                    locale={customLocale} // Koristi prilagođenu lokalizaciju
                                    viewDidMount={(args) => {
                                        updateTitle(args.view); // Ažuriranje naslova
                                    }}
                                    dayHeaderContent={(args) => {
                                        // Skraćeni nazivi dana na latinici
                                        const customDayNamesShort = ['Ned', 'Pon', 'Uto', 'Sre', 'Čet', 'Pet', 'Sub'];
                                        const dayIndex = args.date.getDay(); // Dobijamo indeks dana
                                        const dayName = customDayNamesShort[dayIndex]; // Odgovarajući naziv dana
                                        const date = args.date.getDate(); // Datum u mesecu
                                        const month = args.date.getMonth() + 1; // Meseci su indeksirani od 0, pa dodajemo 1

                                        // Format: "Sre 12.03"
                                        return `${dayName} ${date.toString().padStart(2, '0')}.${month.toString().padStart(2, '0')}`;
                                    }}
                                    allDaySlot={false}
                                    slotMinTime="07:00:00"
                                    slotMaxTime="23:00:00"
                                    slotDuration="01:00:00" // Interval je sada sat vremena
                                    events={termini}
                                    dateClick={handleDateClick}
                                    eventClick={handleEventClick}
                                    eventClassNames={eventClassNames} // Dodato za prilagođene klase
                                    initialDate={selectedDate} // Postavljanje početnog datuma na današnji dan
                                    validRange={validRange}
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
                                    datesSet={handleDatesSet} // Dodaj ovu liniju
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
                                                    {user.type === 9 || user.type === 8 ?
                                                        <Button variant="contained" color="primary" onClick={handleOtkaziNoFree}>
                                                            Otkaži i zaduži
                                                        </Button> : null
                                                    }
                                                    {selectedEvent.type === 0 && (user.type === 9 || user.type === 8) ?
                                                        <Button variant="contained" color="primary" onClick={handleOtkaziFiksni}>
                                                            Otkaži fiksni
                                                        </Button> : null
                                                    }
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
