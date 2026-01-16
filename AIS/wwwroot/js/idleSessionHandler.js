$(document).ready(function () {
    const idleDurationSecs = 3600;    // X number of seconds
    let idleTimeout; // variable to hold the timeout, do not modify

    const resetIdleTimeout = function () {

        // Clears the existing timeout
        if (idleTimeout) clearTimeout(idleTimeout);

        // Set a new idle timeout to load the redirectUrl after idleDurationSecs
        idleTimeout = setTimeout(() => terminateUserSession(), idleDurationSecs * 1000);
    };

    // Init on page load
    resetIdleTimeout();

    // Reset the idle timeout on any of the events listed below
    ['click', 'touchstart', 'mousemove'].forEach(evt =>
        document.addEventListener(evt, resetIdleTimeout, false)
    );
});
function routeToLogin() {
    window.location.href = g_asiBaseURL + "/Login";
}
function terminateUserSession() {
    
    $.ajax({
        url: g_asiBaseURL + "/ApiCalls/terminate_idle_session",
        type: "POST",
        data: {
         
        },
        cache: false,
        success: function (data) {
            alert('Due to inactivity, your session has been terminated');
            onAlertCallback(routeToLogin);
        },
        dataType: "json",
    });
}