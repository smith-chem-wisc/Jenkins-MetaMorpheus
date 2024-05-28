# Send an HTML email with an embedded image and a plain text message for
# email clients that don't want to display the HTML.

import os
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

# Create the root message and fill in the from, to, and subject headers
msgRoot = MIMEMultipart('related')
msgRoot['Subject'] = 'Jenkins Build Results'
msgRoot['From'] = strFrom
msgRoot['To'] = strTo
msgRoot.preamble = 'This is a multi-part message in MIME format.'

# Encapsulate the plain and HTML versions of the message body in an
# 'alternative' part, so message agents can decide which they want to display.
msgAlternative = MIMEMultipart('alternative')
msgRoot.attach(msgAlternative)

msgText = MIMEText('Unable to load graph.')
msgAlternative.attach(msgText)

# We reference the image in the IMG SRC attribute by the ID we give it below
# HTML content for the email with images displayed in a grid
html_content = '''
<b>Daily Jenkins Report</b><br>
<table style="width:100%; max-width:640px; border-collapse: separate; border-spacing: 10px;">
  <tr>
    <td style="padding: 5px;"><img src="cid:image1" style="width:100%; border:1px solid #ddd;" /></td>
    <td style="padding: 5px;"><img src="cid:image2" style="width:100%; border:1px solid #ddd;" /></td>
    <td style="padding: 5px;"><img src="cid:image3" style="width:100%; border:1px solid #ddd;" /></td>
  </tr>
  <tr>
    <td style="padding: 5px;"><img src="cid:image4" style="width:100%; border:1px solid #ddd;" /></td>
    <td style="padding: 5px;"><img src="cid:image5" style="width:100%; border:1px solid #ddd;" /></td>
    <td style="padding: 5px;"><img src="cid:image6" style="width:100%; border:1px solid #ddd;" /></td>
  </tr>
  <tr>
    <td style="padding: 5px;"><img src="cid:image7" style="width:100%; border:1px solid #ddd;" /></td>
    <td style="padding: 5px;"><img src="cid:image8" style="width:100%; border:1px solid #ddd;" /></td>
    <td style="padding: 5px;"><img src="cid:image9" style="width:100%; border:1px solid #ddd;" /></td>
    <td style="padding: 5px;"><img src="cid:image10" style="width:100%; border:1px solid #ddd;" /></td>
  </tr>
</table><br>
'''
msgText = MIMEText(html_content, 'html')
msgAlternative.attach(msgText)

# List of images to attach
image_paths = [
    'D:/Jenkins_Runs/Results/PSMReport.png',
    'D:/Jenkins_Runs/Results/PeptideReport.png',
    'D:/Jenkins_Runs/Results/TaskTimeReport.png',
    'D:/Jenkins_Runs/Results/PSMReport_TopDown.png',
    'D:/Jenkins_Runs/Results/ProteoformReport_TopDown.png',
    'D:/Jenkins_Runs/Results/TaskTimeReport_TopDown.png',
    'D:/Jenkins_Runs/Results/PSMReport_CrossLink.png',
    'D:/Jenkins_Runs/Results/PSMReport_SemiNonModern.png',
    'D:/Jenkins_Runs/Results/PeptideReport_SemiNonModernXL.png',
    'D:/Jenkins_Runs/Results/TaskTimeReport_SemiNonModernXL.png'
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