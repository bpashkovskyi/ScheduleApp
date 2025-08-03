// Main site JavaScript file
// jQuery is loaded from CDN in the layout

$(document).ready(function() {
    // Any global site functionality can go here
    console.log('Site JavaScript loaded');
    
    // Schedule Application JavaScript
    initializeScheduleApp();
});

function initializeScheduleApp() {
    // Initialize date fields with current period
    updateDateFields();
    
    // Initially disable date pickers (they will be enabled if custom is selected)
    $('#fromDate').prop('disabled', true);
    $('#toDate').prop('disabled', true);
    
    // Handle period selection change
    $('#selectedPeriod').change(function() {
        updateDateFields();
    });
    
    // Handle block selection change
    $('#selectedBlock').change(function() {
        loadRooms($(this).val());
    });
    
    // Handle form submission
    $('#scheduleForm').submit(function(e) {
        e.preventDefault();
        loadSchedule();
    });
}

function updateDateFields() {
    const selectedOption = $('#selectedPeriod option:selected');
    const fromDate = selectedOption.data('from');
    const toDate = selectedOption.data('to');
    const periodValue = selectedOption.val();
    
    $('#fromDate').val(fromDate);
    $('#toDate').val(toDate);
    
    // Enable/disable date pickers based on period selection
    if (periodValue === 'custom') {
        $('#fromDate').prop('disabled', false);
        $('#toDate').prop('disabled', false);
    } else {
        $('#fromDate').prop('disabled', true);
        $('#toDate').prop('disabled', true);
    }
}

function loadRooms(blockName) {
    if (!blockName) {
        $('#selectedRoom').html('<option value="">Спочатку оберіть корпус</option>').prop('disabled', true);
        return;
    }
    
    $('#selectedRoom').prop('disabled', true);
    
    $.post('/Home/GetRooms', { selectedBlock: blockName })
        .done(function(response) {
            if (response.success) {
                let options = '<option value="">Оберіть аудиторію</option>';
                response.rooms.forEach(function(room) {
                    options += `<option value="${room.ID}">${room.name}</option>`;
                });
                $('#selectedRoom').html(options).prop('disabled', false);
            } else {
                showError('Помилка завантаження аудиторій: ' + response.error);
            }
        })
        .fail(function() {
            showError('Помилка з\'єднання з сервером');
        });
}

function loadSchedule() {
    const roomId = $('#selectedRoom').val();
    const fromDate = $('#fromDate').val();
    const toDate = $('#toDate').val();
    
    if (!roomId || !fromDate || !toDate) {
        showError('Будь ласка, заповніть всі поля');
        return;
    }
    
    const searchBtn = $('#searchBtn');
    const spinner = searchBtn.find('.spinner-border');
    
    searchBtn.prop('disabled', true);
    spinner.removeClass('d-none');

    $('.card').removeClass('d-none');        
    
    $.ajax({
        url: '/Home/GetSchedule',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({
            roomId: roomId,
            fromDate: fromDate,
            toDate: toDate
        }),
        success: function(response) {
            if (response.success) {
                displaySchedule(response.schedule, response.roomName, response.exportUrl);
            } else {
                showError('Помилка завантаження розкладу:\n' + response.error);
            }
        },
        error: function(xhr, status, error) {
            let errorMessage = 'Помилка з\'єднання з сервером';
            
            // Try to parse error response from server
            if (xhr.responseText) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    if (response.error) {
                        errorMessage = response.error;
                    }
                } catch (e) {
                    // If parsing fails, use default message
                }
            }
            
            showError(errorMessage);
        },
        complete: function() {
            searchBtn.prop('disabled', false);
            spinner.addClass('d-none');
        }
    });
}

function displaySchedule(schedule, roomName, exportUrl) {
    if (Object.keys(schedule).length === 0) {
        // Hide the entire card body when no schedule exists
    } else {
        // Show the card body and display schedule
        $('#exportSection').removeClass('d-none');        

        let html = `<h4 class="mb-3">Аудиторія: ${roomName}</h4>`;
        html += '<div class="schedule-grid">';
        
        const sortedDates = Object.keys(schedule).sort();
        const dates = [];
        
        // Group dates into pairs for 2-column layout
        for (let i = 0; i < sortedDates.length; i += 2) {
            dates.push(sortedDates.slice(i, i + 2));
        }
        
        dates.forEach(function(datePair) {
            html += '<div class="schedule-row">';
            
            datePair.forEach(function(date) {
                const items = schedule[date];
                // Parse date in dd.MM.yyyy format
                const dateParts = date.split('.');
                const dateObj = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]); // year, month (0-indexed), day
                const dayName = dateObj.toLocaleDateString('uk-UA', { weekday: 'long' });
                const formattedDate = dateObj.toLocaleDateString('uk-UA', { 
                    day: '2-digit', 
                    month: '2-digit', 
                    year: 'numeric' 
                });
                
                html += `
                    <div class="schedule-day">
                        <h6 class="day-header">
                            <span class="day-name">${dayName}</span>
                            <span class="day-date">${formattedDate}</span>
                        </h6>
                        <div class="schedule-items">
                `;
                
                items.sort((a, b) => a.lesson_time.localeCompare(b.lesson_time)).forEach(function(item) {
                    // Parse time slot to extract start and end times
                    const timeParts = item.lesson_time.split('-');
                    const startTime = timeParts[0] || item.lesson_time;
                    const endTime = timeParts[1] || '';
                    
                    html += `
                        <div class="schedule-item">
                            <div class="lesson-row">
                                <div class="lesson-name">${item.lesson_name}</div>
                                <div class="time-slot">
                                    <div class="start-time">${startTime}</div>
                                    ${endTime ? `<div class="end-time">${endTime}</div>` : ''}
                                </div>
                                <div class="lesson-description">${item.lesson_description}</div>
                            </div>
                            ${item.comment ? `<div class="lesson-comment">${item.comment}</div>` : ''}
                        </div>
                    `;
                });
                
                html += '</div></div>';
            });
            
            // Add empty column if odd number of days
            if (datePair.length === 1) {
                html += '<div class="schedule-day empty"></div>';
            }
            
            html += '</div>';
        });
        
        html += '</div>';
        
        $('#scheduleContent').html(html);
        $('#exportLink').attr('href', exportUrl);
        $('.card').removeClass('d-none');
    }
}

function showError(message) {
    $('#scheduleContent').html(`
        <div class="alert alert-danger" role="alert">
            ${message}
        </div>
    `);
    $('#exportSection').addClass('d-none');
} 