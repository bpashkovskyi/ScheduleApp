$(document).ready(function() {
    let blocks = [];
    let departments = [];
    let groupDepartments = [];
    let periodOptions = [];

    // Initialize the application
    initializeApp();

    function initializeApp() {
        loadBlocks();
        loadDepartments();
        loadGroupDepartments();
        setupPeriodOptions();
        setupEventHandlers();
        initializeDatePickers();
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

    function loadDepartments() {
        $.ajax({
            url: '/api/schedule/proxy',
            method: 'GET',
            data: {
                q: 'req_type=obj_list&req_mode=teacher&show_ID=yes&req_format=json&coding_mode=UTF8&bs=ok'
            },
            success: function(data) {
                console.log('Departments API response:', data);
                
                // Check for API errors in response body
                if (data.psrozklad_export && data.psrozklad_export.error) {
                    const errorMsg = data.psrozklad_export.error.error_message || 'Невідома помилка API';
                    const errorCode = data.psrozklad_export.error.errorcode || '';
                    showError(`Помилка API (код: ${errorCode}): ${errorMsg}`);
                    return;
                }
                
                // Parse the raw JSON response from external API
                if (data.psrozklad_export && data.psrozklad_export.departments) {
                    departments = data.psrozklad_export.departments.filter(d => d.objects && d.objects.length > 0);
                    populateDepartments();
                } else {
                    showError('Неправильна структура відповіді API: ' + JSON.stringify(data));
                }
            },
            error: function(xhr, status, error) {
                console.error('Departments API error:', {xhr, status, error});
                const errorDetails = getDetailedErrorMessage(xhr);
                showError('Помилка завантаження підрозділів: ' + errorDetails);
            }
        });
    }

    function loadGroupDepartments() {
        $.ajax({
            url: '/api/schedule/proxy',
            method: 'GET',
            data: {
                q: 'req_type=obj_list&req_mode=group&show_ID=yes&req_format=json&coding_mode=UTF8&bs=ok'
            },
            success: function(data) {
                console.log('Group Departments API response:', data);
                
                // Check for API errors in response body
                if (data.psrozklad_export && data.psrozklad_export.error) {
                    const errorMsg = data.psrozklad_export.error.error_message || 'Невідома помилка API';
                    const errorCode = data.psrozklad_export.error.errorcode || '';
                    showError(`Помилка API (код: ${errorCode}): ${errorMsg}`);
                    return;
                }
                
                // Parse the raw JSON response from external API
                if (data.psrozklad_export && data.psrozklad_export.departments) {
                    groupDepartments = data.psrozklad_export.departments.filter(d => d.objects && d.objects.length > 0);
                    populateGroupDepartments();
                } else {
                    showError('Неправильна структура відповіді API: ' + JSON.stringify(data));
                }
            },
            error: function(xhr, status, error) {
                console.error('Group Departments API error:', {xhr, status, error});
                const errorDetails = getDetailedErrorMessage(xhr);
                showError('Помилка завантаження груп: ' + errorDetails);
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

    function populateDepartments() {
        const $departmentSelect = $('#selectedDepartment');
        $departmentSelect.empty();
        $departmentSelect.append('<option value="">Оберіть підрозділ</option>');
        
        departments.forEach(function(department) {
            $departmentSelect.append(`<option value="${department.name}">${department.name}</option>`);
        });
    }

    function populateGroupDepartments() {
        const $groupDepartmentSelect = $('#groupsSelectedDepartment');
        $groupDepartmentSelect.empty();
        $groupDepartmentSelect.append('<option value="">Оберіть підрозділ</option>');
        
        groupDepartments.forEach(function(department) {
            $groupDepartmentSelect.append(`<option value="${department.name}">${department.name}</option>`);
        });
    }

    function populatePeriodOptions() {
        // Populate for rooms
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
        
        // Populate for teachers
        const $teachersPeriodSelect = $('#teachersSelectedPeriod');
        $teachersPeriodSelect.empty();
        
        periodOptions.forEach(function(period) {
            $teachersPeriodSelect.append(`
                <option value="${period.value}" 
                        data-from="${period.fromDate}" 
                        data-to="${period.toDate}">
                    ${period.label}
                </option>
            `);
        });

        // Populate for groups
        const $groupsPeriodSelect = $('#groupsSelectedPeriod');
        $groupsPeriodSelect.empty();
        
        periodOptions.forEach(function(period) {
            $groupsPeriodSelect.append(`
                <option value="${period.value}" 
                        data-from="${period.fromDate}" 
                        data-to="${period.toDate}">
                    ${period.label}
                </option>
            `);
        });
        
        // Set default selection for all
        $periodSelect.val('to_end_of_week');
        $teachersPeriodSelect.val('to_end_of_week');
        $groupsPeriodSelect.val('to_end_of_week');
        updateDateFields();
        toggleDateInputs();
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

        // Department selection change
        $('#selectedDepartment').on('change', function() {
            const selectedDepartmentName = $(this).val();
            if (selectedDepartmentName) {
                populateTeachers(selectedDepartmentName);
            } else {
                resetTeachers();
            }
        });

        // Group Department selection change
        $('#groupsSelectedDepartment').on('change', function() {
            const selectedDepartmentName = $(this).val();
            if (selectedDepartmentName) {
                populateGroups(selectedDepartmentName);
            } else {
                resetGroups();
            }
        });

        // Period selection change for rooms
        $('#selectedPeriod').on('change', function() {
            updateDateFields();
            toggleDateInputs();
        });

        // Period selection change for teachers
        $('#teachersSelectedPeriod').on('change', function() {
            updateTeachersDateFields();
            toggleTeachersDateInputs();
        });

        // Period selection change for groups
        $('#groupsSelectedPeriod').on('change', function() {
            updateGroupsDateFields();
            toggleGroupsDateInputs();
        });

        // Form submissions
        $('#roomsScheduleForm').on('submit', function(e) {
            e.preventDefault();
            searchRoomsSchedule();
        });

        $('#teachersScheduleForm').on('submit', function(e) {
            e.preventDefault();
            searchTeachersSchedule();
        });

        $('#groupsScheduleForm').on('submit', function(e) {
            e.preventDefault();
            searchGroupsSchedule();
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

    function populateTeachers(departmentName) {
        const selectedDepartment = departments.find(d => d.name === departmentName);
        const $teacherSelect = $('#selectedTeacher');
        
        $teacherSelect.empty();
        $teacherSelect.append('<option value="">Оберіть викладача</option>');
        
        if (selectedDepartment && selectedDepartment.objects) {
            selectedDepartment.objects.forEach(function(teacher) {
                const teacherName = `${teacher.P} ${teacher.I} ${teacher.B}`;
                $teacherSelect.append(`<option value="${teacher.ID}">${teacherName}</option>`);
            });
        }
        
        $teacherSelect.prop('disabled', false);
    }

    function populateGroups(departmentName) {
        const selectedDepartment = groupDepartments.find(d => d.name === departmentName);
        const $groupSelect = $('#selectedGroup');
        
        $groupSelect.empty();
        $groupSelect.append('<option value="">Оберіть групу</option>');
        
        if (selectedDepartment && selectedDepartment.objects) {
            selectedDepartment.objects.forEach(function(group) {
                $groupSelect.append(`<option value="${group.ID}">${group.name}</option>`);
            });
        }
        
        $groupSelect.prop('disabled', false);
    }

    function resetGroups() {
        const $groupSelect = $('#selectedGroup');
        $groupSelect.empty();
        $groupSelect.append('<option value="">Спочатку оберіть підрозділ</option>');
        $groupSelect.prop('disabled', true);
    }

    function updateDateFields() {
        const selectedPeriod = $('#selectedPeriod option:selected');
        const fromDate = selectedPeriod.data('from');
        const toDate = selectedPeriod.data('to');
        
        if (fromDate && toDate) {
            // Set dd.MM.yyyy format directly for Flatpickr
            $('#fromDate').val(fromDate);
            $('#toDate').val(toDate);
            
            // Update Flatpickr instances
            if (window.fromDatePicker) {
                window.fromDatePicker.setDate(fromDate, false, 'd.m.Y');
            }
            if (window.toDatePicker) {
                window.toDatePicker.setDate(toDate, false, 'd.m.Y');
            }
        }
    }

    function updateTeachersDateFields() {
        const selectedPeriod = $('#teachersSelectedPeriod option:selected');
        const fromDate = selectedPeriod.data('from');
        const toDate = selectedPeriod.data('to');
        
        if (fromDate && toDate) {
            // Set dd.MM.yyyy format directly for Flatpickr
            $('#teachersFromDate').val(fromDate);
            $('#teachersToDate').val(toDate);
            
            // Update Flatpickr instances
            if (window.teachersFromDatePicker) {
                window.teachersFromDatePicker.setDate(fromDate, false, 'd.m.Y');
            }
            if (window.teachersToDatePicker) {
                window.teachersToDatePicker.setDate(toDate, false, 'd.m.Y');
            }
        }
    }

    function updateGroupsDateFields() {
        const selectedPeriod = $('#groupsSelectedPeriod option:selected');
        const fromDate = selectedPeriod.data('from');
        const toDate = selectedPeriod.data('to');
        
        if (fromDate && toDate) {
            // Set dd.MM.yyyy format directly for Flatpickr
            $('#groupsFromDate').val(fromDate);
            $('#groupsToDate').val(toDate);
            
            // Update Flatpickr instances
            if (window.groupsFromDatePicker) {
                window.groupsFromDatePicker.setDate(fromDate, false, 'd.m.Y');
            }
            if (window.groupsToDatePicker) {
                window.groupsToDatePicker.setDate(toDate, false, 'd.m.Y');
            }
        }
    }

    function toggleDateInputs() {
        const selectedPeriod = $('#selectedPeriod').val();
        const isCustomPeriod = selectedPeriod === 'custom';
        
        // Enable/disable date inputs based on period selection
        $('#fromDate').prop('disabled', !isCustomPeriod);
        $('#toDate').prop('disabled', !isCustomPeriod);
        
        // Update Flatpickr instances
        if (window.fromDatePicker) {
            if (isCustomPeriod) {
                window.fromDatePicker.enable();
            } else {
                window.fromDatePicker.disable();
            }
        }
        
        if (window.toDatePicker) {
            if (isCustomPeriod) {
                window.toDatePicker.enable();
            } else {
                window.toDatePicker.disable();
            }
        }
    }

    function toggleTeachersDateInputs() {
        const selectedPeriod = $('#teachersSelectedPeriod').val();
        const isCustomPeriod = selectedPeriod === 'custom';
        
        // Enable/disable date inputs based on period selection
        $('#teachersFromDate').prop('disabled', !isCustomPeriod);
        $('#teachersToDate').prop('disabled', !isCustomPeriod);
        
        // Update Flatpickr instances
        if (window.teachersFromDatePicker) {
            if (isCustomPeriod) {
                window.teachersFromDatePicker.enable();
            } else {
                window.teachersFromDatePicker.disable();
            }
        }
        
        if (window.teachersToDatePicker) {
            if (isCustomPeriod) {
                window.teachersToDatePicker.enable();
            } else {
                window.teachersToDatePicker.disable();
            }
        }
    }

    function toggleGroupsDateInputs() {
        const selectedPeriod = $('#groupsSelectedPeriod').val();
        const isCustomPeriod = selectedPeriod === 'custom';
        
        // Enable/disable date inputs based on period selection
        $('#groupsFromDate').prop('disabled', !isCustomPeriod);
        $('#groupsToDate').prop('disabled', !isCustomPeriod);
        
        // Update Flatpickr instances
        if (window.groupsFromDatePicker) {
            if (isCustomPeriod) {
                window.groupsFromDatePicker.enable();
            } else {
                window.groupsFromDatePicker.disable();
            }
        }
        
        if (window.groupsToDatePicker) {
            if (isCustomPeriod) {
                window.groupsToDatePicker.enable();
            } else {
                window.groupsToDatePicker.disable();
            }
        }
    }

    function searchRoomsSchedule() {
        const roomId = $('#selectedRoom').val();
        const fromDate = $('#fromDate').val();
        const toDate = $('#toDate').val();

        if (!roomId || !fromDate || !toDate) {
            showError('Необхідно заповнити всі поля');
            return;
        }

        showLoading(true, 'rooms');

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
                console.log('Rooms Schedule API response:', data);
                
                // Check for API errors in response body
                if (data.psrozklad_export && data.psrozklad_export.error) {
                    const errorMsg = data.psrozklad_export.error.error_message || 'Невідома помилка API';
                    const errorCode = data.psrozklad_export.error.errorcode || '';
                    showError(`Помилка API (код: ${errorCode}): ${errorMsg}`);
                    showLoading(false, 'rooms');
                    return;
                }
                
                displayRoomsSchedule(data, roomId);
                showLoading(false, 'rooms');
            },
            error: function(xhr, status, error) {
                console.error('Rooms Schedule API error:', {xhr, status, error});
                const errorDetails = getDetailedErrorMessage(xhr);
                showError('Помилка завантаження розкладу: ' + errorDetails);
                showLoading(false, 'rooms');
            }
        });
    }

    function searchTeachersSchedule() {
        const teacherId = $('#selectedTeacher').val();
        const fromDate = $('#teachersFromDate').val();
        const toDate = $('#teachersToDate').val();

        if (!teacherId || !fromDate || !toDate) {
            showError('Необхідно заповнити всі поля');
            return;
        }

        showLoading(true, 'teachers');

        // Convert HTML date format (yyyy-MM-dd) to API format (dd.MM.yyyy)
        const fromDateAPI = convertDateForAPI(fromDate);
        const toDateAPI = convertDateForAPI(toDate);

        const queryString = `req_type=rozklad&req_mode=teacher&OBJ_ID=${teacherId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDateAPI}&end_date=${toDateAPI}&req_format=json&coding_mode=UTF8&bs=ok`;

        $.ajax({
            url: '/api/schedule/proxy',
            method: 'GET',
            data: {
                q: queryString
            },
            success: function(data) {
                console.log('Teachers Schedule API response:', data);
                
                // Check for API errors in response body
                if (data.psrozklad_export && data.psrozklad_export.error) {
                    const errorMsg = data.psrozklad_export.error.error_message || 'Невідома помилка API';
                    const errorCode = data.psrozklad_export.error.errorcode || '';
                    showError(`Помилка API (код: ${errorCode}): ${errorMsg}`);
                    showLoading(false, 'teachers');
                    return;
                }
                
                displayTeachersSchedule(data, teacherId);
                showLoading(false, 'teachers');
            },
            error: function(xhr, status, error) {
                console.error('Teachers Schedule API error:', {xhr, status, error});
                const errorDetails = getDetailedErrorMessage(xhr);
                showError('Помилка завантаження розкладу: ' + errorDetails);
                showLoading(false, 'teachers');
            }
        });
    }

    function searchGroupsSchedule() {
        const groupId = $('#selectedGroup').val();
        const fromDate = $('#groupsFromDate').val();
        const toDate = $('#groupsToDate').val();

        if (!groupId || !fromDate || !toDate) {
            showError('Необхідно заповнити всі поля');
            return;
        }

        showLoading(true, 'groups');

        // Convert HTML date format (yyyy-MM-dd) to API format (dd.MM.yyyy)
        const fromDateAPI = convertDateForAPI(fromDate);
        const toDateAPI = convertDateForAPI(toDate);

        const queryString = `req_type=rozklad&req_mode=group&OBJ_ID=${groupId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDateAPI}&end_date=${toDateAPI}&req_format=json&coding_mode=UTF8&bs=ok`;

        $.ajax({
            url: '/api/schedule/proxy',
            method: 'GET',
            data: {
                q: queryString
            },
            success: function(data) {
                console.log('Groups Schedule API response:', data);
                
                // Check for API errors in response body
                if (data.psrozklad_export && data.psrozklad_export.error) {
                    const errorMsg = data.psrozklad_export.error.error_message || 'Невідома помилка API';
                    const errorCode = data.psrozklad_export.error.errorcode || '';
                    showError(`Помилка API (код: ${errorCode}): ${errorMsg}`);
                    showLoading(false, 'groups');
                    return;
                }
                
                displayGroupsSchedule(data, groupId);
                showLoading(false, 'groups');
            },
            error: function(xhr, status, error) {
                console.error('Groups Schedule API error:', {xhr, status, error});
                const errorDetails = getDetailedErrorMessage(xhr);
                showError('Помилка завантаження розкладу: ' + errorDetails);
                showLoading(false, 'groups');
            }
        });
    }

    function formatDateForAPI(date) {
        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = date.getFullYear();
        return `${day}.${month}.${year}`; // dd.MM.yyyy for API
    }

    function initializeDatePickers() {
        // Initialize Flatpickr for date inputs with dd.MM.yyyy format
        const datePickerConfig = {
            dateFormat: 'd.m.Y',
            locale: 'uk',
            allowInput: true,
            clickOpens: true,
            placeholder: 'дд.мм.рррр',
            disableMobile: false
        };

        // Rooms date pickers
        window.fromDatePicker = flatpickr('#fromDate', {
            ...datePickerConfig,
            onChange: function(selectedDates, dateStr) {
                // Ensure the input shows the correct format
                $('#fromDate').val(dateStr);
            }
        });

        window.toDatePicker = flatpickr('#toDate', {
            ...datePickerConfig,
            onChange: function(selectedDates, dateStr) {
                // Ensure the input shows the correct format
                $('#toDate').val(dateStr);
            }
        });

        // Teachers date pickers
        window.teachersFromDatePicker = flatpickr('#teachersFromDate', {
            ...datePickerConfig,
            onChange: function(selectedDates, dateStr) {
                // Ensure the input shows the correct format
                $('#teachersFromDate').val(dateStr);
            }
        });

        window.teachersToDatePicker = flatpickr('#teachersToDate', {
            ...datePickerConfig,
            onChange: function(selectedDates, dateStr) {
                // Ensure the input shows the correct format
                $('#teachersToDate').val(dateStr);
            }
        });

        // Groups date pickers
        window.groupsFromDatePicker = flatpickr('#groupsFromDate', {
            ...datePickerConfig,
            onChange: function(selectedDates, dateStr) {
                // Ensure the input shows the correct format
                $('#groupsFromDate').val(dateStr);
            }
        });

        window.groupsToDatePicker = flatpickr('#groupsToDate', {
            ...datePickerConfig,
            onChange: function(selectedDates, dateStr) {
                // Ensure the input shows the correct format
                $('#groupsToDate').val(dateStr);
            }
        });
    }

    function convertDateForAPI(htmlDate) {
        // No conversion needed - Flatpickr already provides dd.MM.yyyy format
        return htmlDate;
    }

    function displayRoomsSchedule(data, roomId) {
        const $scheduleCard = $('#roomsScheduleCard');
        const $scheduleContent = $('#roomsScheduleContent');
        const $exportSection = $('#roomsExportSection');
        const $exportLink = $('#roomsExportLink');

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
            
            const sortedDates = Object.keys(groupedSchedule).sort(function(a, b) {
                // Convert dd.MM.yyyy format to Date objects for proper sorting
                const datePartsA = a.split('.');
                const datePartsB = b.split('.');
                const dateA = new Date(datePartsA[2], datePartsA[1] - 1, datePartsA[0]);
                const dateB = new Date(datePartsB[2], datePartsB[1] - 1, datePartsB[0]);
                return dateA - dateB;
            });
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

    function displayTeachersSchedule(data, teacherId) {
        const $scheduleCard = $('#teachersScheduleCard');
        const $scheduleContent = $('#teachersScheduleContent');
        const $exportSection = $('#teachersExportSection');
        const $exportLink = $('#teachersExportLink');

        // Get export URL
        const fromDate = $('#teachersFromDate').val();
        const toDate = $('#teachersToDate').val();
        const fromDateAPI = convertDateForAPI(fromDate);
        const toDateAPI = convertDateForAPI(toDate);
        const exportQueryString = `req_type=rozklad&req_mode=teacher&OBJ_ID=${teacherId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDateAPI}&end_date=${toDateAPI}&req_format=iCal&coding_mode=UTF8&bs=ok`;
        const exportUrl = `/api/schedule/proxy?q=${encodeURIComponent(exportQueryString)}`;
        
        // Set export link directly to the proxy URL
        $exportLink.attr('href', exportUrl);
        $exportSection.removeClass('d-none');

        // Display schedule
        if (data.psrozklad_export && data.psrozklad_export.roz_items && data.psrozklad_export.roz_items.length > 0) {
            const groupedSchedule = groupScheduleByDate(data.psrozklad_export.roz_items);
            const teacherName = getTeacherName(teacherId);
            
            let scheduleHtml = `<h5 class="mb-3">Розклад для: ${teacherName}</h5>`;
            scheduleHtml += '<div class="row">';
            
            const sortedDates = Object.keys(groupedSchedule).sort(function(a, b) {
                // Convert dd.MM.yyyy format to Date objects for proper sorting
                const datePartsA = a.split('.');
                const datePartsB = b.split('.');
                const dateA = new Date(datePartsA[2], datePartsA[1] - 1, datePartsA[0]);
                const dateB = new Date(datePartsB[2], datePartsB[1] - 1, datePartsB[0]);
                return dateA - dateB;
            });
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

    function displayGroupsSchedule(data, groupId) {
        const $scheduleCard = $('#groupsScheduleCard');
        const $scheduleContent = $('#groupsScheduleContent');
        const $exportSection = $('#groupsExportSection');
        const $exportLink = $('#groupsExportLink');

        // Get export URL
        const fromDate = $('#groupsFromDate').val();
        const toDate = $('#groupsToDate').val();
        const fromDateAPI = convertDateForAPI(fromDate);
        const toDateAPI = convertDateForAPI(toDate);
        const exportQueryString = `req_type=rozklad&req_mode=group&OBJ_ID=${groupId}&OBJ_name=&dep_name=&ros_text=united&begin_date=${fromDateAPI}&end_date=${toDateAPI}&req_format=iCal&coding_mode=UTF8&bs=ok`;
        const exportUrl = `/api/schedule/proxy?q=${encodeURIComponent(exportQueryString)}`;
        
        // Set export link directly to the proxy URL
        $exportLink.attr('href', exportUrl);
        $exportSection.removeClass('d-none');

        // Display schedule
        if (data.psrozklad_export && data.psrozklad_export.roz_items && data.psrozklad_export.roz_items.length > 0) {
            const groupedSchedule = groupScheduleByDate(data.psrozklad_export.roz_items);
            const groupName = getGroupName(groupId);
            
            let scheduleHtml = `<h5 class="mb-3">Розклад для: ${groupName}</h5>`;
            scheduleHtml += '<div class="row">';
            
            const sortedDates = Object.keys(groupedSchedule).sort(function(a, b) {
                // Convert dd.MM.yyyy format to Date objects for proper sorting
                const datePartsA = a.split('.');
                const datePartsB = b.split('.');
                const dateA = new Date(datePartsA[2], datePartsA[1] - 1, datePartsA[0]);
                const dateB = new Date(datePartsB[2], datePartsB[1] - 1, datePartsB[0]);
                return dateA - dateB;
            });
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

    function getTeacherName(teacherId) {
        for (const department of departments) {
            if (department.objects) {
                const teacher = department.objects.find(t => t.ID === teacherId);
                if (teacher) {
                    return `${teacher.P} ${teacher.I} ${teacher.B}`;
                }
            }
        }
        return 'Невідомий викладач';
    }

    function getGroupName(groupId) {
        for (const department of groupDepartments) {
            if (department.objects) {
                const group = department.objects.find(g => g.ID === groupId);
                if (group) {
                    return group.name;
                }
            }
        }
        return 'Невідома група';
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

    function showLoading(show, type = 'rooms') {
        let $spinner, $buttonText, $button;
        
        if (type === 'rooms') {
            $spinner = $('#searchRoomsBtn .spinner-border');
            $button = $('#searchRoomsBtn');
        } else if (type === 'teachers') {
            $spinner = $('#searchTeachersBtn .spinner-border');
            $button = $('#searchTeachersBtn');
        } else if (type === 'groups') {
            $spinner = $('#searchGroupsBtn .spinner-border');
            $button = $('#searchGroupsBtn');
        }
        
        $buttonText = $button.contents().filter(function() {
            return this.nodeType === 3;
        });

        if (show) {
            $spinner.removeClass('d-none');
            $buttonText.text(' Завантаження...');
            $button.prop('disabled', true);
        } else {
            $spinner.addClass('d-none');
            $buttonText.text('Знайти розклад');
            $button.prop('disabled', false);
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