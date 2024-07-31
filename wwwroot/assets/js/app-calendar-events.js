// Define and calculate dates
const currentDate = new Date();
const nextDay = new Date(currentDate.getTime() + 864e5); // 864e5 is 1 day in milliseconds
const nextMonth = currentDate.getMonth() === 11 ? new Date(currentDate.getFullYear() + 1, 0, 1) : new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, 1);
const prevMonth = currentDate.getMonth() === 11 ? new Date(currentDate.getFullYear() - 1, 0, 1) : new Date(currentDate.getFullYear(), currentDate.getMonth() - 1, 1);

// Event data
const events = [
  {
    id: 1,
    url: "",
    title: "Design Review",
    start: currentDate,
    end: nextDay,
    allDay: false,
    extendedProps: { calendar: "Business" }
  },
  {
    id: 2,
    url: "",
    title: "Meeting With Client",
    start: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -11),
    end: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -10),
    allDay: true,
    extendedProps: { calendar: "Business" }
  },
  {
    id: 3,
    url: "",
    title: "Family Trip",
    allDay: true,
    start: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -9),
    end: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -7),
    extendedProps: { calendar: "Holiday" }
  },
  {
    id: 4,
    url: "",
    title: "Doctor's Appointment",
    start: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -11),
    end: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -10),
    extendedProps: { calendar: "Personal" }
  },
  {
    id: 5,
    url: "",
    title: "Dart Game?",
    start: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -13),
    end: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -12),
    allDay: true,
    extendedProps: { calendar: "ETC" }
  },
  {
    id: 6,
    url: "",
    title: "Meditation",
    start: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -13),
    end: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -12),
    allDay: true,
    extendedProps: { calendar: "Personal" }
  },
  {
    id: 7,
    url: "",
    title: "Dinner",
    start: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -13),
    end: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -12),
    extendedProps: { calendar: "Family" }
  },
  {
    id: 8,
    url: "",
    title: "Product Review",
    start: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -13),
    end: new Date(currentDate.getFullYear(), currentDate.getMonth() + 1, -12),
    allDay: true,
    extendedProps: { calendar: "Business" }
  },
  {
    id: 9,
    url: "",
    title: "Monthly Meeting",
    start: nextMonth,
    end: nextMonth,
    allDay: true,
    extendedProps: { calendar: "Business" }
  },
  {
    id: 10,
    url: "",
    title: "Monthly Checkup",
    start: prevMonth,
    end: prevMonth,
    allDay: true,
    extendedProps: { calendar: "Personal" }
  }
];

// Assign the events array to the window object to make it globally accessible
window.events = events;
