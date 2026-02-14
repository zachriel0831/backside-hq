async function login() {

    const accountInput = document.getElementById('account');
    const passwordInput = document.getElementById('password');

    const result = await libs.api.post('/account/login', {
        data: { account: accountInput.value, password: passwordInput.value },
    });

    const successCode = Number(result && result.code);
    if (successCode === 1) {
        const redirectUrl =
            (result && result.data && result.data.redirectUrl) ||
            (result && result.redirectUrl) ||
            '/Home/Index';

        window.location.assign(redirectUrl);
        return;
    }

    alert((result && result.message) || '登入失敗，請稍後再試');
}

document.addEventListener('DOMContentLoaded', function() {
    const accountInput = document.getElementById('account');
    const passwordInput = document.getElementById('password');

    if (accountInput) {
        accountInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                login();
            }
        });
    }

    if (passwordInput) {
        passwordInput.addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                login();
            }
        });
    }
});