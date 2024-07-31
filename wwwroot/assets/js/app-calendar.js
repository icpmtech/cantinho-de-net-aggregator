const apiUrl = '/api/events';

// Fetch events from the API
async function fetchEvents() {
  try {
    const response = await fetch(apiUrl);
    return await response.json();
  } catch (error) {
    console.error('Error fetching events:', error);
    return [];
  }
}

// Set assetsPath to '~/assets'
const assetsPath = '~/assets';

// Validate assets path
function validateAssetsPath(path) {
  if (!path || typeof path !== 'string' || !path.trim()) {
    console.error('Error: Invalid assetsPath. Please ensure it is set correctly.');
    return false;
  }
  return true;
}

let direction = "ltr"; // Direction is explicitly set to LTR

document.addEventListener("DOMContentLoaded", async function () {
  if (!validateAssetsPath(assetsPath)) {
    return; // Exit if assetsPath is invalid
  }

  let eventsList = await fetchEvents();

  let calendarElement = document.getElementById("calendar"),
    sidebarElement = document.querySelector(".app-calendar-sidebar"),
    addEventSidebarElement = document.getElementById("addEventSidebar"),
    overlayElement = document.querySelector(".app-overlay"),
    eventCategories = {
      Business: "primary",
      Holiday: "success",
      Personal: "danger",
      Family: "warning",
      ETC: "info"
    },
    offcanvasTitleElement = document.querySelector(".offcanvas-title"),
    toggleSidebarButton = document.querySelector(".btn-toggle-sidebar"),
    addEventButton = document.querySelector("#addEventBtn"),
    deleteEventButton = document.querySelector(".btn-delete-event"),
    cancelButton = document.querySelector(".btn-cancel"),
    eventTitleInput = document.querySelector("#eventTitle"),
    eventStartDateInput = document.querySelector("#eventStartDate"),
    eventEndDateInput = document.querySelector("#eventEndDate"),
    eventURLInput = document.querySelector("#eventURL"),
    eventLabelSelect = $("#eventLabel"),
    eventGuestsSelect = $("#eventGuests"),
    eventLocationInput = document.querySelector("#eventLocation"),
    eventDescriptionInput = document.querySelector("#eventDescription"),
    eventImpactInput = document.querySelector("#eventImpact"),
    eventSentimentInput = document.querySelector("#eventSentiment"),
    eventSourceInput = document.querySelector("#eventSource"),
    eventPriceInput = document.querySelector("#eventPrice"),
    eventPriceChangeInput = document.querySelector("#eventPriceChange"),
    eventPortfolioItemIdInput = document.querySelector("#eventPortfolioItemId"),
    allDaySwitch = document.querySelector(".allDay-switch"),
    selectAllCheckbox = document.querySelector(".select-all"),
    filterInputs = [].slice.call(document.querySelectorAll(".input-filter")),
    inlineCalendarElement = document.querySelector(".inline-calendar"),
    selectedEvent,
    isFormValid = false,
    inlineCalendarInstance,
    offcanvasInstance = new bootstrap.Offcanvas(addEventSidebarElement);

  function formatEventLabel(option) {
    return option.id ? "<span class='badge badge-dot bg-" + $(option.element).data("label") + " me-2'> </span>" + option.text : option.text;
  }

  function formatGuestOption(option) {
    return option.id ? "<div class='d-flex flex-wrap align-items-center'><div class='avatar avatar-xs me-2'><img src='" + assetsPath + "/img/avatars/" + $(option.element).data("avatar") + "' alt='avatar' class='rounded-circle' /></div>" + option.text + "</div>" : option.text;
  }

  var startDatePicker, endDatePicker;

  function customizeSidebarToggleButton() {
    var sidebarToggleButton = document.querySelector(".fc-sidebarToggle-button");
    if (sidebarToggleButton) {
      sidebarToggleButton.classList.remove("fc-button-primary");
      sidebarToggleButton.classList.add("d-lg-none", "d-inline-block", "ps-0");

      while (sidebarToggleButton.firstChild) {
        sidebarToggleButton.firstChild.remove();
      }

      sidebarToggleButton.setAttribute("data-bs-toggle", "sidebar");
      sidebarToggleButton.setAttribute("data-overlay", "");
      sidebarToggleButton.setAttribute("data-target", "#app-calendar-sidebar");
      sidebarToggleButton.insertAdjacentHTML("beforeend", '<i class="bx bx-menu bx-lg text-heading"></i>');
    }
  }

  if (eventLabelSelect.length) {
    eventLabelSelect.wrap('<div class="position-relative"></div>').select2({
      placeholder: "Select value",
      dropdownParent: eventLabelSelect.parent(),
      templateResult: formatEventLabel,
      templateSelection: formatEventLabel,
      minimumResultsForSearch: -1,
      escapeMarkup: function (markup) {
        return markup;
      }
    });
  }

  if (eventGuestsSelect.length) {
    eventGuestsSelect.wrap('<div class="position-relative"></div>').select2({
      placeholder: "Select value",
      dropdownParent: eventGuestsSelect.parent(),
      closeOnSelect: false,
      templateResult: formatGuestOption,
      templateSelection: formatGuestOption,
      escapeMarkup: function (markup) {
        return markup;
      }
    });
  }

  if (eventStartDateInput) {
    startDatePicker = eventStartDateInput.flatpickr({
      enableTime: true,
      altFormat: "Y-m-dTH:i:S",
      onReady: function (dates, dateStr, instance) {
        if (instance.isMobile) {
          instance.mobileInput.setAttribute("step", null);
        }
      }
    });
  }

  if (eventEndDateInput) {
    endDatePicker = eventEndDateInput.flatpickr({
      enableTime: true,
      altFormat: "Y-m-dTH:i:S",
      onReady: function (dates, dateStr, instance) {
        if (instance.isMobile) {
          instance.mobileInput.setAttribute("step", null);
        }
      }
    });
  }

  if (inlineCalendarElement) {
    inlineCalendarInstance = inlineCalendarElement.flatpickr({
      monthSelectorType: "static",
      inline: true
    });
  }

  let calendarInstance = new Calendar(calendarElement, {
    initialView: "dayGridMonth",
    events: function (fetchInfo, successCallback) {
      let selectedCategories = [].slice.call(document.querySelectorAll(".input-filter:checked")).map(input => input.getAttribute("data-value"));
      let filteredEvents = eventsList.filter(event => {
        return event.calendar && selectedCategories.includes(event.calendar.toLowerCase());
      });
      successCallback(filteredEvents);
    },
    plugins: [dayGridPlugin, interactionPlugin, listPlugin, timegridPlugin],
    editable: true,
    dragScroll: true,
    dayMaxEvents: 2,
    eventResizableFromStart: true,
    customButtons: {
      sidebarToggle: {
        text: "Sidebar"
      }
    },
    headerToolbar: {
      start: "sidebarToggle, prev,next, title",
      end: "dayGridMonth,timeGridWeek,timeGridDay,listMonth"
    },
    direction: direction,
    initialDate: new Date(),
    navLinks: true,
    eventClassNames: function ({ event }) {
      return ["fc-event-" + eventCategories[event.calendar]];
    },
    dateClick: function (info) {
      let selectedDate = moment(info.date).format("YYYY-MM-DD");
      resetEventForm();
      offcanvasInstance.show();
      if (offcanvasTitleElement) offcanvasTitleElement.innerHTML = "Add Event";
      addEventButton.innerHTML = "Add";
      addEventButton.classList.remove("btn-update-event");
      addEventButton.classList.add("btn-add-event");
      deleteEventButton.classList.add("d-none");
      eventStartDateInput.value = selectedDate;
      eventEndDateInput.value = selectedDate;
    },
    eventClick: function (info) {
      selectedEvent = info.event;
      if (selectedEvent.url) {
        info.jsEvent.preventDefault();
        window.open(selectedEvent.url, "_blank");
      }
      offcanvasInstance.show();
      if (offcanvasTitleElement) offcanvasTitleElement.innerHTML = "Update Event";
      addEventButton.innerHTML = "Update";
      addEventButton.classList.add("btn-update-event");
      addEventButton.classList.remove("btn-add-event");
      deleteEventButton.classList.remove("d-none");
      eventTitleInput.value = selectedEvent.title;
      startDatePicker.setDate(selectedEvent.start, true, "Y-m-d");
      allDaySwitch.checked = !!selectedEvent.allDay;
      if (selectedEvent.end) {
        endDatePicker.setDate(selectedEvent.end, true, "Y-m-d");
      } else {
        endDatePicker.setDate(selectedEvent.start, true, "Y-m-d");
      }
      eventLabelSelect.val(selectedEvent.calendar).trigger("change");
      if (selectedEvent.location) {
        eventLocationInput.value = selectedEvent.location;
      }
      if (selectedEvent.guests) {
        eventGuestsSelect.val(selectedEvent.guests).trigger("change");
      }
      if (selectedEvent.description) {
        eventDescriptionInput.value = selectedEvent.description;
      }
      if (selectedEvent.extendedProps) {
        eventImpactInput.value = selectedEvent.extendedProps.impact;
        eventSentimentInput.value = selectedEvent.extendedProps.sentiment;
        eventSourceInput.value = selectedEvent.extendedProps.source;
        eventPriceInput.value = selectedEvent.extendedProps.price;
        eventPriceChangeInput.value = selectedEvent.extendedProps.priceChange;
        eventPortfolioItemIdInput.value = selectedEvent.extendedProps.portfolioItemId;
      }
    },
    datesSet: function () {
      customizeSidebarToggleButton();
    },
    viewDidMount: function () {
      customizeSidebarToggleButton();
    }
  });

  calendarInstance.render();
  customizeSidebarToggleButton();

  var eventForm = document.getElementById("eventForm");

  function resetEventForm() {
    eventEndDateInput.value = "";
    eventURLInput.value = "";
    eventStartDateInput.value = "";
    eventTitleInput.value = "";
    eventLocationInput.value = "";
    allDaySwitch.checked = false;
    eventGuestsSelect.val("").trigger("change");
    eventDescriptionInput.value = "";
    eventImpactInput.value = "";
    eventSentimentInput.value = "";
    eventSourceInput.value = "";
    eventPriceInput.value = "";
    eventPriceChangeInput.value = "";
    eventPortfolioItemIdInput.value = "";
  }

  FormValidation.formValidation(eventForm, {
    fields: {
      eventTitle: {
        validators: {
          notEmpty: {
            message: "Please enter event title"
          }
        }
      },
      eventStartDate: {
        validators: {
          notEmpty: {
            message: "Please enter start date"
          }
        }
      },
      eventEndDate: {
        validators: {
          notEmpty: {
            message: "Please enter end date"
          }
        }
      }
    },
    plugins: {
      trigger: new FormValidation.plugins.Trigger(),
      bootstrap5: new FormValidation.plugins.Bootstrap5({
        eleValidClass: "",
        rowSelector: function (field, element) {
          return ".mb-6";
        }
      }),
      submitButton: new FormValidation.plugins.SubmitButton(),
      autoFocus: new FormValidation.plugins.AutoFocus()
    }
  }).on("core.form.valid", function () {
    isFormValid = true;
  }).on("core.form.invalid", function () {
    isFormValid = false;
  });

  if (toggleSidebarButton) {
    toggleSidebarButton.addEventListener("click", () => {
      cancelButton.classList.remove("d-none");
    });
  }

  addEventButton.addEventListener("click", async () => {
    if (addEventButton.classList.contains("btn-add-event") && isFormValid) {
      let newEvent = {
        title: eventTitleInput.value,
        start: eventStartDateInput.value,
        end: eventEndDateInput.value,
        url: eventURLInput.value,
        location: eventLocationInput.value,
        guests: eventGuestsSelect.val(),
        calendar: eventLabelSelect.val(),
        description: eventDescriptionInput.value,
        allDay: allDaySwitch.checked,
        extendedProps: {
          impact: eventImpactInput.value,
          sentiment: eventSentimentInput.value,
          source: eventSourceInput.value,
          price: eventPriceInput.value,
          priceChange: eventPriceChangeInput.value,
          portfolioItemId: eventPortfolioItemIdInput.value
        }
      };

      let response = await fetch(apiUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(newEvent)
      });

      if (response.ok) {
        eventsList = await fetchEvents();
        calendarInstance.refetchEvents();
        offcanvasInstance.hide();
      } else {
        console.error('Error saving event:', response.statusText);
      }
    } else if (isFormValid) {
      let updatedEvent = {
        id: selectedEvent.id,
        title: eventTitleInput.value,
        start: eventStartDateInput.value,
        end: eventEndDateInput.value,
        url: eventURLInput.value,
        location: eventLocationInput.value,
        guests: eventGuestsSelect.val(),
        calendar: eventLabelSelect.val(),
        description: eventDescriptionInput.value,
        allDay: !!allDaySwitch.checked,
        extendedProps: {
          impact: eventImpactInput.value,
          sentiment: eventSentimentInput.value,
          source: eventSourceInput.value,
          price: eventPriceInput.value,
          priceChange: eventPriceChangeInput.value,
          portfolioItemId: eventPortfolioItemIdInput.value
        }
      };

      let response = await fetch(`${apiUrl}/${selectedEvent.id}`, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(updatedEvent)
      });

      if (response.ok) {
        eventsList = await fetchEvents();
        calendarInstance.refetchEvents();
        offcanvasInstance.hide();
      } else {
        console.error('Error updating event:', response.statusText);
      }
    }
  });

  deleteEventButton.addEventListener("click", async () => {
    let response = await fetch(`${apiUrl}/${selectedEvent.id}`, {
      method: 'DELETE'
    });

    if (response.ok) {
      eventsList = await fetchEvents();
      calendarInstance.refetchEvents();
      offcanvasInstance.hide();
    } else {
      console.error('Error deleting event:', response.statusText);
    }
  });

  addEventSidebarElement.addEventListener("hidden.bs.offcanvas", resetEventForm);

  toggleSidebarButton.addEventListener("click", () => {
    if (offcanvasTitleElement) offcanvasTitleElement.innerHTML = "Add Event";
    addEventButton.innerHTML = "Add";
    addEventButton.classList.remove("btn-update-event");
    addEventButton.classList.add("btn-add-event");
    deleteEventButton.classList.add("d-none");
    sidebarElement.classList.remove("show");
    overlayElement.classList.remove("show");
  });

  if (selectAllCheckbox) {
    selectAllCheckbox.addEventListener("click", (event) => {
      if (event.currentTarget.checked) {
        document.querySelectorAll(".input-filter").forEach(input => input.checked = true);
      } else {
        document.querySelectorAll(".input-filter").forEach(input => input.checked = false);
      }
      calendarInstance.refetchEvents();
    });
  }

  if (filterInputs) {
    filterInputs.forEach(input => {
      input.addEventListener("click", () => {
        if (document.querySelectorAll(".input-filter:checked").length < document.querySelectorAll(".input-filter").length) {
          selectAllCheckbox.checked = false;
        } else {
          selectAllCheckbox.checked = true;
        }
        calendarInstance.refetchEvents();
      });
    });
  }

  if (inlineCalendarInstance) {
    inlineCalendarInstance.config.onChange.push(function (selectedDates) {
      calendarInstance.changeView(calendarInstance.view.type, moment(selectedDates[0]).format("YYYY-MM-DD"));
      customizeSidebarToggleButton();
      sidebarElement.classList.remove("show");
      overlayElement.classList.remove("show");
    });
  }
});
