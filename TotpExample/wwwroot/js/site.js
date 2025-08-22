(async function main() {
    document.getElementById('btnEnroll').addEventListener('click', async function () {
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
        var responseJson = await response.json();
        document.getElementById('imgQR').src = `data:image/png;base64,${responseJson.data.qrCode}`;
    });

    document.getElementById('btnValidate').addEventListener('click', async function () {
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
        document.getElementById('spResult').innerText = response.status == 200 ? 'Success!' : 'Failed';
    });
})();