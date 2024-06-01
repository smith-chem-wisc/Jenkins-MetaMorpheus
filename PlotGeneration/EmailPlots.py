# Send an HTML email with an embedded image and a plain text message for
# email clients that don't want to display the HTML.

import os
import re
import requests
import sys
from datetime import date
from email.MIMEMultipart import MIMEMultipart
from email.MIMEText import MIMEText
from email.MIMEImage import MIMEImage

# Define these once; use them twice!
folder = os.path.dirname(os.path.realpath(__file__))
email = [x.strip() for x in open(os.path.join(folder, 'email.config')).readlines()]
strFrom = email[0]
strTo = email[0]
email_server_host = 'smtp.gmail.com'
port = 587
email_username = strFrom
email_password = email[1]
reportdirectory = str(sys.argv[1])
date = date.today().strftime("%Y/%m/%d")
reportType = 'Monthly' if "Monthly" in reportdirectory else 'Daily'

# Create the root message and fill in the from, to, and subject headers
msgRoot = MIMEMultipart('related')
msgRoot['Subject'] = '{reportType} Jenkins Build Results {date}'.format(date = date, reportType=reportType)
msgRoot['From'] = strFrom
msgRoot['To'] = strTo
msgRoot.preamble = 'This is a multi-part message in MIME format.'

# Encapsulate the plain and HTML versions of the message body in an
# 'alternative' part, so message agents can decide which they want to display.
msgAlternative = MIMEMultipart('alternative')
msgRoot.attach(msgAlternative)

msgText = MIMEText('Unable to load graph.')
msgAlternative.attach(msgText)

# Extract MetaMorpheus and MzLib Version Numbers
def extract_version(file_path):
    with open(file_path, 'r') as file:
        for line in file:
            # Check if the line contains the mzLib package reference
            if '<PackageReference Include="mzLib"' in line:
                # Use a regular expression to extract the version number
                match = re.search(r'Version="([\d\.]+)"', line)
                if match:
                    return match.group(1)
    return None

file_path = os.path.join(os.path.dirname(folder),'MetaMorpheus_MasterBranch\\MetaMorpheus\\GUI\\GUI.csproj')
mzLibVersion = extract_version(file_path)

response = requests.get("https://api.github.com/repos/smith-chem-wisc/MetaMorpheus/releases/latest")
mmVersion = response.json()["tag_name"]
response = requests.get("https://api.github.com/repos/smith-chem-wisc/MetaMorpheus/commits/master")
latestCommitMessage = response.json()["commit"]["message"]
latestCommitHash = response.json()["sha"]

# We reference the image in the IMG SRC attribute by the ID we give it below
# HTML content for the email with images displayed in a grid
html_content = '''
<b>Daily Jenkins Report</b><br>
<p>MetaMorpheus Version: {mmVersion}</p>
<p>MetaMorpheus Latest Commit Message: {latestCommitMessage}</p>
<p>MetaMorpheus Latest Commit Hash: {latestCommitHash}</p>
<p>Installed MzLib Version: {mzLibVersion}</p>
<div style="transform: scale(0.6); transform-origin: top left; width: 160%; margin-left: -20%;">
<table style="width:100%; max-width:640px; border-spacing: 10px; border-collapse: separate;">
  <tr>
    <td style="width:33.33%;"><img src="cid:image1" style="width:100%;" /></td>
    <td style="width:33.33%;"><img src="cid:image2" style="width:100%;" /></td>
    <td style="width:33.33%;"><img src="cid:image3" style="width:100%;" /></td>
  </tr>
  <tr>
    <td style="width:33.33%;"><img src="cid:image4" style="width:100%;" /></td>
    <td style="width:33.33%;"><img src="cid:image5" style="width:100%;" /></td>
    <td style="width:33.33%;"><img src="cid:image6" style="width:100%;" /></td>
  </tr>
  <tr>
    <td style="width:33.33%;"><img src="cid:image7" style="width:100%;" /></td>
    <td style="width:33.33%;"><img src="cid:image8" style="width:100%;" /></td>
    <td style="width:33.33%;"><img src="cid:image9" style="width:100%;" /></td>
  </tr>
  <tr>
    <td style="width:33.33%;"><img src="cid:image10" style="width:100%;" /></td>
  </tr>
</table><br>
</div>
'''.format(mmVersion = mmVersion, latestCommitMessage=latestCommitMessage, latestCommitHash=latestCommitHash,mzLibVersion=mzLibVersion)

msgText = MIMEText(html_content, 'html')
msgAlternative.attach(msgText)

# List of images to attach
image_paths = [
    os.path.join(reportdirectory, 'PSMReport.png'),
    os.path.join(reportdirectory, 'TaskTimeReport.png'),
    os.path.join(reportdirectory, 'PeptideReport.png'),
    os.path.join(reportdirectory, 'PSMReport_TopDown.png'),
    os.path.join(reportdirectory, 'TaskTimeReport_TopDown.png'),
    os.path.join(reportdirectory, 'ProteoformReport_TopDown.png'),
    os.path.join(reportdirectory, 'PSMReport_CrossLink.png'),
    os.path.join(reportdirectory, 'TaskTimeReport_SemiNonModernXLGlyco.png'),
    os.path.join(reportdirectory, 'PeptideReport_SemiNonModern.png'),
    os.path.join(reportdirectory, 'PSMReport_SemiNonModernGlyco.png'),
]

# Attach images to the email
for i, image_path in enumerate(image_paths):
    with open(image_path, 'rb') as fp:
        msgImage = MIMEImage(fp.read())
    msgImage.add_header('Content-ID', '<image{}>'.format(i+1))
    msgRoot.attach(msgImage)

# Send the email to every address listed in the email.config (this example assumes SMTP authentication is required)
import smtplib
smtp = smtplib.SMTP(email_server_host, port)
smtp.ehlo()
smtp.starttls()
smtp.login(email_username, email_password)
for recipient in email:
    if recipient.__eq__(email_password):
        continue
    smtp.sendmail(strFrom, recipient, msgRoot.as_string())
smtp.close()