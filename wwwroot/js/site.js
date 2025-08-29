document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.counter-control').forEach(function (control) {
        const input = control.querySelector('input');
        const minusBtn = control.querySelector('.btn-counter-minus');
        const plusBtn = control.querySelector('.btn-counter-plus');

        minusBtn.addEventListener('click', function () {
            let value = parseInt(input.value);
            if (value > 0) {
                input.value = value - 1;
            }
        });

        plusBtn.addEventListener('click', function () {
            let value = parseInt(input.value);
            input.value = value + 1;
        });
    });
});