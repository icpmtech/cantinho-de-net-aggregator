async function captureChartAndSend(chartElementId, apiUrl, responseDivId) {
  const chartElement = document.getElementById(chartElementId);
  const responseDiv = document.getElementById(responseDivId);
  if (!chartElement) {
    console.error("Chart element not found!");
    return;
  }
  responseDiv.innerHTML = `
        <div id="loading" style="text-align: center; font-size: 16px;">
            <p>Uploading and analyzing chart...</p>
            <div class="spinner" style="width: 30px; height: 30px; border: 4px solid #ccc; border-top: 4px solid #007bff; border-radius: 50%; animation: spin 1s linear infinite;"></div>
        </div>
    `;
  try {
    // Capture the chart as an image using html2canvas
    const canvas = await html2canvas(chartElement);
    const imageBlob = await new Promise(resolve => canvas.toBlob(resolve, "image/png"));

    // Create a FormData object to send the image
    const formData = new FormData();
    formData.append("UploaderName", "none");
    formData.append("UploaderAddress", "Analyzer");
    formData.append("File", imageBlob);
    // Send the image to the specified API
    const response = await fetch(apiUrl, {
      method: "POST",
      body: formData
    });
    if (response.ok) {
      const result = await response.json();

      // Display success message with analysis
      responseDiv.innerHTML = `
                        <div class="alert  text-black  alert-dismissible fade show" role="alert">
                            <strong>Analisys Status:</strong> Success<br>
                            <strong>Analysis:</strong> ${result.analysis}
                            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                        </div>
                    `;
    } else {
      // Display error message
      responseDiv.innerHTML = `
                        <div class="alert alert-danger alert-dismissible fade show" role="alert">
                            <strong>Analisys Status:</strong> Failed<br>
                            <strong>Error:</strong> ${response.statusText}
                            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                        </div>
                    `;
    }
  } catch (error) {
    // Display error message
    responseDiv.innerHTML = `
                    <div class="alert alert-danger alert-dismissible fade show" role="alert">
                        <strong>Analisys Status:</strong> Error<br>
                        <strong>Error Details:</strong> ${error.message}
                        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                    </div>
                `;
  }
}
