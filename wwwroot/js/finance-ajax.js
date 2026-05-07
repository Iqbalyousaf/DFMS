$(function() {
    function reloadExpenditures(empId) {
        $.get('/Finance/ExpendituresPartial', { employeeId: empId })
            .done(function(html) {
                $('#expenditures-container').html(html);
            });
    }

    function reloadIncomes(empId) {
        $.get('/Finance/IncomesPartial', { employeeId: empId })
            .done(function(html) {
                $('#incomes-container').html(html);
            });
    }

    // when the employee select changes (form submit already reloads whole page), but in case
    // we want ajax reload, listen to change on select with name employeeId
    $(document).on('change', 'select[name="employeeId"]', function() {
        var empId = $(this).val();
        reloadExpenditures(empId);
        reloadIncomes(empId);
    });

    // Intercept submission of AddExpenditure and AddIncome to perform AJAX submit
    $(document).on('submit', 'form[asp-action="AddExpenditure"]', function(e) {
        e.preventDefault();
        var $form = $(this);
        $.post($form.attr('action'), $form.serialize())
            .done(function() {
                var empId = $form.find('input[name="EmpId"]').val();
                reloadExpenditures(empId);
            });
    });

    $(document).on('submit', 'form[asp-action="AddIncome"]', function(e) {
        e.preventDefault();
        var $form = $(this);
        $.post($form.attr('action'), $form.serialize())
            .done(function() {
                var empId = $form.find('input[name="EmpId"]').val();
                reloadIncomes(empId);
            });
    });

    // Intercept delete forms inside partials
    $(document).on('submit', '.ajax-delete-exp', function(e) {
        e.preventDefault();
        var $form = $(this);
        if (!confirm('Delete?')) return;
        $.post($form.attr('action'), $form.serialize())
            .done(function() {
                var empId = $form.find('input[name="empId"]').val();
                reloadExpenditures(empId);
            });
    });

    $(document).on('submit', '.ajax-delete-inc', function(e) {
        e.preventDefault();
        var $form = $(this);
        if (!confirm('Delete?')) return;
        $.post($form.attr('action'), $form.serialize())
            .done(function() {
                var empId = $form.find('input[name="empId"]').val();
                reloadIncomes(empId);
            });
    });
});
