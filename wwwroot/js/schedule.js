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
                // Parse the raw JSON response from external API
                if (data.psrozklad_export && data.psrozklad_export.blocks) {
                    blocks = data.psrozklad_export.blocks.filter(b => b.objects && b.objects.length > 0);
                    populateBlocks();
                }
            },
            error: function(xhr) {
                showError('Помилка завантаження корпусів: ' + getErrorMessage(xhr));
            }
        });
    }

    function setupPeriodOptions() {
        const today = new Date();
        const currentWeekStart = getWeekStart(today);
        const currentWeekEnd = getWeekEnd(today);
        const currentMonthStart = new Date(today.getFullYear(), today.getMonth(), 1);
        const currentMonthEnd = new Date(today.getFullYear(), today.getMonth() + 1, 0);
        const previousMonthStart = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        const previousMonthEnd = new Date(today.getFullYear(), today.getMonth(), 0);
        
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
                $roomSelect.append(`<option value="${room.id}">${room.name}</option>`);
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

        const queryString = `req_type=rozklad&req_mode=room&OBJ_ID=${roomId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDate}&end_date=${toDate}&req_format=json&coding_mode=UTF8&bs=ok`;

        $.ajax({
            url: '/api/schedule/proxy',
            method: 'GET',
            data: {
                q: queryString
            },
            success: function(data) {
                displaySchedule(data, roomId);
                showLoading(false);
            },
            error: function(xhr) {
                showError('Помилка завантаження розкладу: ' + getErrorMessage(xhr));
                showLoading(false);
            }
        });
    }

    function displaySchedule(data, roomId) {
        const $scheduleCard = $('#scheduleCard');
        const $scheduleContent = $('#scheduleContent');
        const $exportSection = $('#exportSection');
        const $exportLink = $('#exportLink');

        // Get export URL
        const fromDate = $('#fromDate').val();
        const toDate = $('#toDate').val();
        const exportQueryString = `req_type=rozklad&req_mode=room&OBJ_ID=${roomId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDate}&end_date=${toDate}&req_format=iCal&coding_mode=UTF8&bs=ok`;
        const exportUrl = `/api/schedule/proxy?q=${encodeURIComponent(exportQueryString)}`;
        
        // Set export link directly to the proxy URL
        $exportLink.attr('href', exportUrl);
        $exportSection.removeClass('d-none');

        // Display schedule
        if (data.psrozklad_export && data.psrozklad_export.rozItems && data.psrozklad_export.rozItems.length > 0) {
            const groupedSchedule = groupScheduleByDate(data.psrozklad_export.rozItems);
            const roomName = getRoomName(roomId);
            
            let scheduleHtml = `<h5 class="mb-3">Розклад для: ${roomName}</h5>`;
            
            Object.keys(groupedSchedule).sort().forEach(function(date) {
                const daySchedule = groupedSchedule[date];
                scheduleHtml += createDayScheduleHtml(date, daySchedule);
            });
            
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
                const room = block.objects.find(r => r.id === roomId);
                if (room) {
                    return room.name;
                }
            }
        }
        return 'Невідома аудиторія';
    }

    function createDayScheduleHtml(date, daySchedule) {
        const dateObj = new Date(date);
        const dayName = getDayName(dateObj.getDay());
        const formattedDate = dateObj.toLocaleDateString('uk-UA', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        });

        let dayHtml = `
            <div class="card mb-3">
                <div class="card-header">
                    <strong>${dayName}, ${formattedDate}</strong>
                </div>
                <div class="card-body">
        `;

        if (daySchedule && daySchedule.length > 0) {
            daySchedule.forEach(function(item) {
                dayHtml += `
                    <div class="schedule-item mb-2 p-2 border rounded">
                        <div class="row">
                            <div class="col-md-2">
                                <strong>${item.time || ''}</strong>
                            </div>
                            <div class="col-md-10">
                                <div><strong>${item.subject || ''}</strong></div>
                                <div class="text-muted small">${item.teacher || ''}</div>
                                <div class="text-muted small">${item.group || ''}</div>
                            </div>
                        </div>
                    </div>
                `;
            });
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

    function formatDateForAPI(date) {
        return date.toISOString().split('T')[0];
    }

    function getWeekStart(date) {
        const dayOfWeek = date.getDay();
        const diff = date.getDate() - dayOfWeek + (dayOfWeek === 0 ? -6 : 1);
        return new Date(date.setDate(diff));
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
        
        // Auto-hide after 5 seconds
        setTimeout(function() {
            $errorAlert.addClass('d-none');
        }, 5000);
    }

    function getErrorMessage(xhr) {
        if (xhr.responseJSON && xhr.responseJSON.error) {
            return xhr.responseJSON.error;
        }
        return xhr.statusText || 'Невідома помилка';
    }
}); 