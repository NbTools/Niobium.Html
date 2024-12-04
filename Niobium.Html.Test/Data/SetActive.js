$(document).ready(function () {
    $('.sidenav a').click(function () {
        $('.sidenav a').removeClass('active');
        $(this).addClass('active');
    })
});
