    // Simple password show/hide (does not interfere with login.js)
    (function(){
        var btn = document.getElementById('togglePasswordBtn');
        var input = document.getElementById('inputPassword');
        if(!btn || !input) return;
        btn.addEventListener('click', function(){
            input.type = (input.type === 'password') ? 'text' : 'password';
        });
    })();
