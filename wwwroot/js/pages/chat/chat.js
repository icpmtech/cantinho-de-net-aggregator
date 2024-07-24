document.addEventListener('DOMContentLoaded', function () {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

  let currentAudio = null;

  connection.on("ReceiveMessage", function (user, message) {
    const msg = document.createElement("div");
    const messageId = `msg-${Date.now()}`;
    msg.setAttribute("id", messageId);
    msg.innerHTML = `<strong>${user}:</strong> ${marked(message)}
                                                    <button class="btn btn-sm btn-outline-secondary" onclick="copyMessage('${messageId}')">Copy</button>
                                                    <button class="btn btn-sm btn-outline-danger" onclick="deleteMessage('${messageId}')">Delete</button>
                                                    <button class="btn btn-sm btn-outline-success" onclick="saveMessage('${messageId}')">Save</button>`;
    document.getElementById("messagesList").appendChild(msg);
    // Convert the transcription to speech and play it
    readAudio(message);
    Prism.highlightAllUnder(msg);
  });

  connection.on("ReceiveFile", function (user, fileUrl, analysis) {
    const msg = document.createElement("div");
    const messageId = `msg-${Date.now()}`;
    msg.setAttribute("id", messageId);
    msg.innerHTML = `<strong>${user}:</strong> <a href="${fileUrl}" target="_blank">${fileUrl}</a>
                                                    <p>Analysis: ${marked(analysis)}</p>
                                                    <button class="btn btn-sm btn-outline-secondary" onclick="copyMessage('${messageId}')">Copy</button>
                                                    <button class="btn btn-sm btn-outline-danger" onclick="deleteMessage('${messageId}')">Delete</button>
                                                    <button class="btn btn-sm btn-outline-success" onclick="saveMessage('${messageId}')">Save</button>`;
    document.getElementById("messagesList").appendChild(msg);
  });

  connection.start().catch(function (err) {
    return console.error(err.toString());
  });

  window.sendMessage = function () {
    const user = document.getElementById("userInput").value;
    if (!user) {
      alert('Please enter your user name.');
      return;
    }

    const message = document.getElementById("messageInput").value;
    if (!message) {
      alert('Please enter a message.');
      return;
    }

    connection.invoke("SendMessage", user, message).catch(function (err) {
      return console.error(err.toString());
    });
  };

  window.sendFile = function () {
    const user = document.getElementById("userInput").value;
    if (!user) {
      alert('Please enter your user name.');
      return;
    }

    const fileInput = document.getElementById("fileInput");
    const file = fileInput.files[0];
    if (!file) {
      alert('Please select a file to upload.');
      return;
    }

    const formData = new FormData();
    formData.append("UploaderName", user);
    formData.append("UploaderAddress", "Analyzer"); // Add an appropriate value or get it from an input field
    formData.append("File", file);

    // Add instructional message to chat
    const msgInstruction = document.createElement("div");
    const instructionId = `msg-instruction-${Date.now()}`;
    msgInstruction.setAttribute("id", instructionId);
    msgInstruction.innerHTML = `<strong>${user}:</strong> Please analyze the content of the following file.`;
    document.getElementById("messagesList").appendChild(msgInstruction);

    // Show "Analyzing file..." message
    const msg = document.createElement("div");
    const messageId = `msg-${Date.now()}`;
    msg.setAttribute("id", messageId);
    msg.innerHTML = `<strong>${user}:</strong> Analyzing file...`;
    document.getElementById("messagesList").appendChild(msg);

    fetch('/api/Chat/UploadFile', {
      method: 'POST',
      body: formData
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (data) {
        if (data.success) {
          connection.invoke("SendFile", user, data.fileUrl, data.analysis).catch(function (err) {
            return console.error(err.toString());
          });

          // Update the "Analyzing file..." message with the analysis result
          const analysisMsg = document.createElement("div");
          const analysisId = `msg-analysis-${Date.now()}`;
          analysisMsg.setAttribute("id", analysisId);
          analysisMsg.innerHTML = `<strong>${user}:</strong> File analysis completed.
                                                             <p>Analysis: ${marked(data.analysis)}</p>
                                                             <button class="btn btn-sm btn-outline-secondary" onclick="copyMessage('${analysisId}')">Copy</button>
                                                             <button class="btn btn-sm btn-outline-danger" onclick="deleteMessage('${analysisId}')">Delete</button>
                                                             <button class="btn btn-sm btn-outline-success" onclick="saveMessage('${analysisId}')">Save</button>`;
          document.getElementById("messagesList").appendChild(analysisMsg);
        } else {
          alert('File upload failed: ' + data.message);
        }
      })
      .catch(function (error) {
        console.error('Error uploading file:', error);
      });
  };

  window.toggleChat = function () {
    const chatWindow = document.getElementById("chatWindow");
    chatWindow.classList.toggle("minimized");
  };

  window.maximizeChat = function () {
    const chatWindow = document.getElementById("chatWindow");
    chatWindow.classList.toggle("maximized");
  };

  window.copyMessage = function (messageId) {
    const messageText = document.getElementById(messageId).innerText;
    navigator.clipboard.writeText(messageText).then(function () {
      alert("Message copied to clipboard");
    });
  };

  window.deleteMessage = function (messageId) {
    const messageElement = document.getElementById(messageId);
    messageElement.parentNode.removeChild(messageElement);
  };

  window.saveMessage = function (messageId) {
    const messageElement = document.getElementById(messageId);
    const messageText = messageElement.innerText;
    fetch('/api/Chat/saveMessage', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ content: messageText })
    })
      .then(function (response) {
        return response.json();
      })
      .then(function (data) {
        alert(`Message saved to ${data.filePath}`);
      })
      .catch(function (error) {
        console.error('Error saving message:', error);
      });
  };

  let mediaRecorder;
  let audioChunks = [];

  window.startRecording = function () {
    const user = document.getElementById("userInput").value;
    if (!user) {
      alert('Please enter your user name.');
      return;
    }

    navigator.mediaDevices.getUserMedia({ audio: true })
      .then(stream => {
        mediaRecorder = new MediaRecorder(stream);
        mediaRecorder.start();
        document.getElementById('stopRecordingButton').style.display = 'inline-block';

        mediaRecorder.addEventListener("dataavailable", event => {
          audioChunks.push(event.data);
        });

        mediaRecorder.addEventListener("stop", () => {
          const audioBlob = new Blob(audioChunks, { type: 'audio/wav' });
          audioChunks = [];

          const formData = new FormData();
          formData.append("UploaderName", user);
          formData.append("UploaderAddress", "Analyzer Audio"); // Add an appropriate value or get it from an input field
          formData.append("File", audioBlob);

          // Show "Analyzing audio..." message
          const msg = document.createElement("div");
          const messageId = `msg-${Date.now()}`;
          msg.setAttribute("id", messageId);
          msg.innerHTML = `<strong>${user}:</strong> Analyzing audio...`;
          document.getElementById("messagesList").appendChild(msg);

          fetch('/api/Chat/UploadAudio', {
            method: 'POST',
            body: formData
          })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                connection.invoke("SendMessage", user, data.transcription).catch(err => {
                  return console.error(err.toString());
                });

                // Update the "Analyzing audio..." message with the transcription result
                const analysisMsg = document.createElement("div");
                const analysisId = `msg-analysis-${Date.now()}`;
                analysisMsg.setAttribute("id", analysisId);
                analysisMsg.innerHTML = `<strong>${user}:</strong> Audio transcription completed.
                                                            <p>Transcription: ${marked(data.transcription)}</p>
                                                            <button class="btn btn-sm btn-outline-secondary" onclick="copyMessage('${analysisId}')">Copy</button>
                                                            <button class="btn btn-sm btn-outline-danger" onclick="deleteMessage('${analysisId}')">Delete</button>
                                                            <button class="btn btn-sm btn-outline-success" onclick="saveMessage('${analysisId}')">Save</button>`;
                document.getElementById("messagesList").appendChild(analysisMsg);


              } else {
                alert('Audio upload failed: ' + data.message);
              }
            })
            .catch(error => {
              console.error('Error uploading audio:', error);
            });
        });

      })
      .catch(error => {
        console.error('Error accessing microphone:', error);
      });
  };

  window.stopRecording = function () {
    mediaRecorder.stop();
    document.getElementById('stopRecordingButton').style.display = 'none';
  };

  window.stopAudio = function () {
    if (currentAudio) {
      currentAudio.pause();
      currentAudio.currentTime = 0;
    }
    document.getElementById('stopAudioButton').style.display = 'none';
  };

  async function readAudio(content) {
    // Stop the current audio if it's playing
    if (currentAudio) {
      currentAudio.pause();
      currentAudio.currentTime = 0;
    }

    const response = await fetch('/api/Chat/ReadAudio', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ Content: content })
    });

    if (!response.ok) {
      console.error('Text to speech conversion failed:', await response.text());
      return;
    }

    const audioBlob = await response.blob();
    const audioUrl = URL.createObjectURL(audioBlob);
    currentAudio = new Audio(audioUrl);
    currentAudio.play();
    document.getElementById('stopAudioButton').style.display = 'inline-block';
  }
});
