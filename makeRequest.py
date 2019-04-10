import requests
import subprocess

accessTokenUrl = "https://demo.identityserver.io/connect/token"

accessTokenPayload = "scopes=api&grant_type=client_credentials&undefined="
accessTokenHeaders = {
    'Content-Type': "application/x-www-form-urlencoded",
    'Authorization': "Basic Y2xpZW50OnNlY3JldA==",
    'cache-control': "no-cache"
    }

response = requests.request("POST", accessTokenUrl, data=accessTokenPayload, headers=accessTokenHeaders)

accessToken = response.json()["access_token"]
print(accessToken)
print("Retrieved access token.")

origin = subprocess.run('minikube service policytest-deployment --url',
    shell=True,
    # Probably don't forget these, too
    check=True, text=True, stdout=subprocess.PIPE).stdout.strip()

url = f"{origin}/api/Values"

payload = ""
headers = {
    'Authorization': f"Bearer {accessToken}",
    'cache-control': "no-cache",
    }

response = requests.request("GET", url, data=payload, headers=headers)

print(response.text)