(async function main() {
    document.getElementById('btnEnroll').addEventListener('click', async function () {

        // Send password to server so that we can use it to encrypt the OTP secret
        const password = document.getElementById('txbPassword').value;
        const data = {
            password: password
        };
        const body = JSON.stringify(data);
        const response = await fetch('/api/totp/enroll', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', },
            body: body
        });

        // Show the QR code so that the user can scan it with their authenticator app
        var responseJson = await response.json();
        document.getElementById('imgQR').src = `data:image/png;base64,${responseJson.data.qrCode}`;

        document.getElementById('secValidate').classList.remove('visually-hidden');
    });

    document.getElementById('btnValidate').addEventListener('click', async function () {

        // Send password and OTP to server
        const password = document.getElementById('txbPassword').value;
        const code = document.getElementById('txbCode').value;
        const data = {
            code: code,
            password: password
        };
        const body = JSON.stringify(data);
        const response = await fetch('/api/totp/validate', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', },
            body: body
        });

        // Show response
        document.getElementById('spResult').innerText = response.status == 200 ? 'Success!' : 'Failed';
    });
})();