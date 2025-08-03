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
    
    $('#fromDate').val(fromDate);
    $('#toDate').val(toDate);
}

function loadRooms(blockName) {
    if (!blockName) {
        $('#selectedRoom').html('<option value="">Спочатку оберіть блок</option>').prop('disabled', true);
        return;
    }
    
    $('#selectedRoom').prop('disabled', true);
    
    $.post('/Home/GetRooms', { selectedBlock: blockName })
        .done(function(response) {
            if (response.success) {
                let options = '<option value="">Оберіть аудиторію</option>';
                response.rooms.forEach(function(room) {
                    options += `<option value="${room.id}">${room.name}</option>`;
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
                showError('Помилка завантаження розкладу: ' + response.error);
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
    let html = `<h6 class="mb-3">Аудиторія: ${roomName}</h6>`;
    
    if (Object.keys(schedule).length === 0) {
        html += '<div class="text-center text-muted py-4"><p>Розклад не знайдено для обраного періоду</p></div>';
    } else {
        html += '<div class="schedule-list">';
        
        Object.keys(schedule).sort().forEach(function(date) {
            const items = schedule[date];
            const dateObj = new Date(date);
            const dayName = dateObj.toLocaleDateString('uk-UA', { weekday: 'long' });
            const formattedDate = dateObj.toLocaleDateString('uk-UA', { 
                day: '2-digit', 
                month: '2-digit', 
                year: 'numeric' 
            });
            
            html += `
                <div class="schedule-day mb-4">
                    <h6 class="day-header">
                        <span class="day-name">${dayName}</span>
                        <span class="day-date">${formattedDate}</span>
                    </h6>
                    <div class="schedule-items">
            `;
            
            items.sort((a, b) => a.Time.localeCompare(b.Time)).forEach(function(item) {
                html += `
                    <div class="schedule-item">
                        <div class="time-slot">${item.Time}</div>
                        <div class="lesson-info">
                            <div class="lesson-name">${item.Lesson}</div>
                            <div class="lesson-details">
                                <span class="teacher">${item.Teacher}</span>
                                <span class="group">${item.Group}</span>
                            </div>
                        </div>
                    </div>
                `;
            });
            
            html += '</div></div>';
        });
        
        html += '</div>';
    }
    
    $('#scheduleContent').html(html);
    $('#exportLink').attr('href', exportUrl);
    $('#exportSection').removeClass('d-none');
}

function showError(message) {
    $('#scheduleContent').html(`
        <div class="alert alert-danger" role="alert">
            ${message}
        </div>
    `);
    $('#exportSection').addClass('d-none');
} 