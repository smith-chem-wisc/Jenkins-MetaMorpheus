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
#msgText = MIMEText('<b>Daily Jenkins Report</b><br><table width="100%" style="max-width:640px;"><tr><td><img src="cid:image1" width="100%" /></td</tr></table><br>', 'html')
msgText = MIMEText('<b>Daily Jenkins Report</b><br><img src="cid:image1"><br><br><img src="cid:image2"><br>', 'html')
msgAlternative.attach(msgText)

# This example assumes the image is in the current directory
fp = open('D:/Jenkins_Runs/Results/PSMReport.png', 'rb')
msgImage = MIMEImage(fp.read())
fp.close()

# Define the image's ID as referenced above
msgImage.add_header('Content-ID', '<image1>')
msgRoot.attach(msgImage)

# This example assumes the image is in the current directory
fp = open('D:/Jenkins_Runs/Results/TaskTimeReport.png', 'rb')
msgImage = MIMEImage(fp.read())
fp.close()

# Define the image's ID as referenced above
msgImage.add_header('Content-ID', '<image2>')
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