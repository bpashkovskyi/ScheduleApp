$(document).ready(function() {
    let blocks = [];
    let periodOptions = [];

    // Initialize the application
    initializeApp();

    function initializeApp() {
        loadBlocks();
        setupPeriodOptions();
        setupEventHandlers();
    }

    function loadBlocks() {
        $.ajax({
            url: '/api/schedule/proxy',
            method: 'GET',
            data: {
                q: 'req_type=obj_list&req_mode=room&show_ID=yes&req_format=json&coding_mode=UTF8&bs=ok'
            },
            success: function(data) {
                console.log('Blocks API response:', data);
                
                // Check for API errors in response body
                if (data.psrozklad_export && data.psrozklad_export.error) {
                    const errorMsg = data.psrozklad_export.error.error_message || 'Невідома помилка API';
                    const errorCode = data.psrozklad_export.error.errorcode || '';
                    showError(`Помилка API (код: ${errorCode}): ${errorMsg}`);
                    return;
                }
                
                // Parse the raw JSON response from external API
                if (data.psrozklad_export && data.psrozklad_export.blocks) {
                    blocks = data.psrozklad_export.blocks.filter(b => b.objects && b.objects.length > 0);
                    populateBlocks();
                } else {
                    showError('Неправильна структура відповіді API: ' + JSON.stringify(data));
                }
            },
            error: function(xhr, status, error) {
                console.error('Blocks API error:', {xhr, status, error});
                const errorDetails = getDetailedErrorMessage(xhr);
                showError('Помилка завантаження корпусів: ' + errorDetails);
            }
        });
    }

    function setupPeriodOptions() {
        const today = new Date();
        const currentWeekStart = getWeekStart(today);
        const currentWeekEnd = getWeekEnd(today);
        
        // Fix current month calculation - ensure we get the correct month
        const currentYear = today.getFullYear();
        const currentMonth = today.getMonth(); // 0-11
        const currentMonthStart = new Date(currentYear, currentMonth, 1);
        const currentMonthEnd = new Date(currentYear, currentMonth + 1, 0);
        
        // Fix previous month calculation
        const previousMonthStart = new Date(currentYear, currentMonth - 1, 1);
        const previousMonthEnd = new Date(currentYear, currentMonth, 0);
        
        const currentTerm = getCurrentTerm(today);
        
        periodOptions = [
            {
                value: 'to_end_of_week',
                label: 'До кінця тижня',
                fromDate: formatDateForAPI(today),
                toDate: formatDateForAPI(currentWeekEnd)
            },
            {
                value: 'current_week',
                label: 'Поточний тиждень',
                fromDate: formatDateForAPI(currentWeekStart),
                toDate: formatDateForAPI(currentWeekEnd)
            },
            {
                value: 'current_month',
                label: 'Поточний місяць',
                fromDate: formatDateForAPI(currentMonthStart),
                toDate: formatDateForAPI(currentMonthEnd)
            },
            {
                value: 'previous_month',
                label: 'Попередній місяць',
                fromDate: formatDateForAPI(previousMonthStart),
                toDate: formatDateForAPI(previousMonthEnd)
            },
            {
                value: 'current_term',
                label: 'Поточний семестр',
                fromDate: formatDateForAPI(currentTerm.start),
                toDate: formatDateForAPI(currentTerm.end)
            },
            {
                value: 'custom',
                label: 'Власний період',
                fromDate: formatDateForAPI(today),
                toDate: formatDateForAPI(today)
            }
        ];
        
        populatePeriodOptions();
    }

    function populateBlocks() {
        const $blockSelect = $('#selectedBlock');
        $blockSelect.empty();
        $blockSelect.append('<option value="">Оберіть корпус</option>');
        
        blocks.forEach(function(block) {
            $blockSelect.append(`<option value="${block.name}">${block.name}</option>`);
        });
    }

    function populatePeriodOptions() {
        const $periodSelect = $('#selectedPeriod');
        $periodSelect.empty();
        
        periodOptions.forEach(function(period) {
            $periodSelect.append(`
                <option value="${period.value}" 
                        data-from="${period.fromDate}" 
                        data-to="${period.toDate}">
                    ${period.label}
                </option>
            `);
        });
        
        // Set default selection
        $periodSelect.val('to_end_of_week');
        updateDateFields();
    }

    function setupEventHandlers() {
        // Block selection change
        $('#selectedBlock').on('change', function() {
            const selectedBlockName = $(this).val();
            if (selectedBlockName) {
                populateRooms(selectedBlockName);
            } else {
                resetRooms();
            }
        });

        // Period selection change
        $('#selectedPeriod').on('change', function() {
            updateDateFields();
        });

        // Form submission
        $('#scheduleForm').on('submit', function(e) {
            e.preventDefault();
            searchSchedule();
        });
    }

    function populateRooms(blockName) {
        const selectedBlock = blocks.find(b => b.name === blockName);
        const $roomSelect = $('#selectedRoom');
        
        $roomSelect.empty();
        $roomSelect.append('<option value="">Оберіть аудиторію</option>');
        
        if (selectedBlock && selectedBlock.objects) {
            selectedBlock.objects.forEach(function(room) {
                $roomSelect.append(`<option value="${room.ID}">${room.name}</option>`);
            });
        }
        
        $roomSelect.prop('disabled', false);
    }

    function resetRooms() {
        const $roomSelect = $('#selectedRoom');
        $roomSelect.empty();
        $roomSelect.append('<option value="">Спочатку оберіть корпус</option>');
        $roomSelect.prop('disabled', true);
    }

    function updateDateFields() {
        const selectedPeriod = $('#selectedPeriod option:selected');
        const fromDate = selectedPeriod.data('from');
        const toDate = selectedPeriod.data('to');
        
        if (fromDate && toDate) {
            $('#fromDate').val(fromDate);
            $('#toDate').val(toDate);
        }
    }

    function searchSchedule() {
        const roomId = $('#selectedRoom').val();
        const fromDate = $('#fromDate').val();
        const toDate = $('#toDate').val();

        if (!roomId || !fromDate || !toDate) {
            showError('Необхідно заповнити всі поля');
            return;
        }

        showLoading(true);

        // Convert HTML date format (yyyy-MM-dd) to API format (dd.MM.yyyy)
        const fromDateAPI = convertDateForAPI(fromDate);
        const toDateAPI = convertDateForAPI(toDate);

        const queryString = `req_type=rozklad&req_mode=room&OBJ_ID=${roomId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDateAPI}&end_date=${toDateAPI}&req_format=json&coding_mode=UTF8&bs=ok`;

        $.ajax({
            url: '/api/schedule/proxy',
            method: 'GET',
            data: {
                q: queryString
            },
            success: function(data) {
                console.log('Schedule API response:', data);
                
                // Check for API errors in response body
                if (data.psrozklad_export && data.psrozklad_export.error) {
                    const errorMsg = data.psrozklad_export.error.error_message || 'Невідома помилка API';
                    const errorCode = data.psrozklad_export.error.errorcode || '';
                    showError(`Помилка API (код: ${errorCode}): ${errorMsg}`);
                    showLoading(false);
                    return;
                }
                
                displaySchedule(data, roomId);
                showLoading(false);
            },
            error: function(xhr, status, error) {
                console.error('Schedule API error:', {xhr, status, error});
                const errorDetails = getDetailedErrorMessage(xhr);
                showError('Помилка завантаження розкладу: ' + errorDetails);
                showLoading(false);
            }
        });
    }

    function formatDateForAPI(date) {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}.${month}.${year}`; // dd.MM.yyyy for HTML text inputs
    }

    function convertDateForAPI(htmlDate) {
        // Convert dd.MM.yyyy to dd.MM.yyyy (no conversion needed now)
        return htmlDate;
    }

    function displaySchedule(data, roomId) {
        const $scheduleCard = $('#scheduleCard');
        const $scheduleContent = $('#scheduleContent');
        const $exportSection = $('#exportSection');
        const $exportLink = $('#exportLink');

        // Get export URL
        const fromDate = $('#fromDate').val();
        const toDate = $('#toDate').val();
        const fromDateAPI = convertDateForAPI(fromDate);
        const toDateAPI = convertDateForAPI(toDate);
        const exportQueryString = `req_type=rozklad&req_mode=room&OBJ_ID=${roomId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDateAPI}&end_date=${toDateAPI}&req_format=iCal&coding_mode=UTF8&bs=ok`;
        const exportUrl = `/api/schedule/proxy?q=${encodeURIComponent(exportQueryString)}`;
        
        // Set export link directly to the proxy URL
        $exportLink.attr('href', exportUrl);
        $exportSection.removeClass('d-none');

        // Display schedule
        if (data.psrozklad_export && data.psrozklad_export.roz_items && data.psrozklad_export.roz_items.length > 0) {
            const groupedSchedule = groupScheduleByDate(data.psrozklad_export.roz_items);
            const roomName = getRoomName(roomId);
            
            let scheduleHtml = `<h5 class="mb-3">Розклад для: ${roomName}</h5>`;
            scheduleHtml += '<div class="row">';
            
            const sortedDates = Object.keys(groupedSchedule).sort();
            sortedDates.forEach(function(date, index) {
                const daySchedule = groupedSchedule[date];
                const colClass = index % 2 === 0 ? 'col-md-6' : 'col-md-6';
                scheduleHtml += `
                    <div class="${colClass} mb-3">
                        ${createDayScheduleHtml(date, daySchedule)}
                    </div>
                `;
            });
            
            scheduleHtml += '</div>';
            $scheduleContent.html(scheduleHtml);
        } else {
            $scheduleContent.html(`
                <div class="text-center text-muted py-5">
                    <i class="bi bi-calendar-x fs-1"></i>
                    <p class="mt-3">На обраний період розклад не знайдено</p>
                </div>
            `);
        }

        $scheduleCard.removeClass('d-none');
    }

    function groupScheduleByDate(scheduleItems) {
        const grouped = {};
        scheduleItems.forEach(function(item) {
            if (!grouped[item.date]) {
                grouped[item.date] = [];
            }
            grouped[item.date].push(item);
        });
        return grouped;
    }

    function getRoomName(roomId) {
        for (const block of blocks) {
            if (block.objects) {
                const room = block.objects.find(r => r.ID === roomId);
                if (room) {
                    return room.name;
                }
            }
        }
        return 'Невідома аудиторія';
    }

    function createDayScheduleHtml(date, daySchedule) {
        // Convert date from dd.MM.yyyy format to Date object
        const dateParts = date.split('.');
        const dateObj = new Date(dateParts[2], dateParts[1] - 1, dateParts[0]);
        const dayName = getDayName(dateObj.getDay());
        const formattedDate = dateObj.toLocaleDateString('uk-UA', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });

        let dayHtml = `
            <div class="card h-100">
                <div class="card-header">
                    <strong>${dayName}, ${formattedDate}</strong>
                </div>
                <div class="card-body">
        `;

        if (daySchedule && daySchedule.length > 0) {
            dayHtml += `
                <div class="table-responsive">
                    <table class="table table-sm">
                        <thead>
                            <tr>
                                <th style="width: 10%">№</th>
                                <th style="width: 15%">Час</th>
                                <th style="width: 75%">Заняття</th>
                            </tr>
                        </thead>
                        <tbody>
            `;
            
            daySchedule.forEach(function(item) {
                dayHtml += `
                    <tr>
                        <td class="text-center fw-bold">${item.lesson_number || ''}</td>
                        <td class="text-primary fw-bold">${item.lesson_time || ''}</td>
                        <td>${item.lesson_description || ''}</td>
                    </tr>
                `;
            });
            
            dayHtml += `
                        </tbody>
                    </table>
                </div>
            `;
        } else {
            dayHtml += '<div class="text-muted">Розклад відсутній</div>';
        }

        dayHtml += '</div></div>';
        return dayHtml;
    }

    function getDayName(dayIndex) {
        const days = ['Неділя', 'Понеділок', 'Вівторок', 'Середа', 'Четвер', 'П\'ятниця', 'Субота'];
        return days[dayIndex];
    }

    function getWeekStart(date) {
        const dayOfWeek = date.getDay();
        const diff = date.getDate() - dayOfWeek + (dayOfWeek === 0 ? -6 : 1);
        return new Date(date.getFullYear(), date.getMonth(), diff);
    }

    function getWeekEnd(date) {
        const weekStart = getWeekStart(new Date(date));
        return new Date(weekStart.getTime() + 6 * 24 * 60 * 60 * 1000);
    }

    function getCurrentTerm(date) {
        const month = date.getMonth() + 1;
        const day = date.getDate();
        
        if ((month === 8 && day >= 20) || month === 9 || month === 10 || month === 11 || month === 12 || (month === 1 && day <= 15)) {
            return {
                start: new Date(date.getFullYear(), 8, 1), // September 1
                end: new Date(date.getFullYear(), 11, 31)  // December 31
            };
        } else {
            return {
                start: new Date(date.getFullYear(), 1, 20), // February 20
                end: new Date(date.getFullYear(), 5, 30)    // June 30
            };
        }
    }

    function showLoading(show) {
        const $spinner = $('#searchBtn .spinner-border');
        const $buttonText = $('#searchBtn').contents().filter(function() {
            return this.nodeType === 3;
        });

        if (show) {
            $spinner.removeClass('d-none');
            $buttonText.text(' Завантаження...');
            $('#searchBtn').prop('disabled', true);
        } else {
            $spinner.addClass('d-none');
            $buttonText.text('Знайти розклад');
            $('#searchBtn').prop('disabled', false);
        }
    }

    function showError(message) {
        const $errorAlert = $('#errorAlert');
        $errorAlert.text(message).removeClass('d-none');
        
        // Make error more visible
        $errorAlert.css({
            'background-color': '#f8d7da',
            'border-color': '#f5c6cb',
            'color': '#721c24',
            'padding': '15px',
            'border-radius': '8px',
            'margin-bottom': '15px',
            'font-weight': '600'
        });
        
        // Auto-hide after 10 seconds (longer for debugging)
        setTimeout(function() {
            $errorAlert.addClass('d-none');
        }, 10000);
        
        // Also log to console for debugging
        console.error('Application Error:', message);
    }

    function getErrorMessage(xhr) {
        if (xhr.responseJSON && xhr.responseJSON.error) {
            return xhr.responseJSON.error;
        }
        return xhr.statusText || 'Невідома помилка';
    }

    function getDetailedErrorMessage(xhr) {
        let errorMessage = 'Невідома помилка';
        if (xhr.responseJSON && xhr.responseJSON.error) {
            errorMessage = xhr.responseJSON.error;
        } else if (xhr.responseJSON && xhr.responseJSON.message) {
            errorMessage = xhr.responseJSON.message;
        } else if (xhr.responseText) {
            errorMessage = xhr.responseText;
        } else if (xhr.statusText) {
            errorMessage = xhr.statusText;
        }
        return `${xhr.status} ${xhr.statusText}: ${errorMessage}`;
    }
}); 