async function captureChartAndSend(chartElementId, apiUrl, responseDivId) {
  // Utility Functions
  function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(() => {
      alert("Text copied to clipboard!");
    }).catch(err => {
      console.error("Failed to copy text: ", err);
    });
  }

  function exportToFile(text) {
    const blob = new Blob([text], { type: "text/plain" });
    const link = document.createElement("a");
    link.href = URL.createObjectURL(blob);
    link.download = "analysis.txt";
    link.click();
    URL.revokeObjectURL(link.href);
  }

  function printAnalysis(text) {
    const printWindow = window.open("", "_blank");
    printWindow.document.write(`<pre>${text}</pre>`);
    printWindow.document.close();
    printWindow.print();
  }

  function markdownToHtml(markdown) {
    // For better Markdown support, consider using a library like 'marked.js'
    return markdown
      .replace(/^######\s(.+)/gm, '<h6>$1</h6>')
      .replace(/^#####\s(.+)/gm, '<h5>$1</h5>')
      .replace(/^####\s(.+)/gm, '<h4>$1</h4>')
      .replace(/^###\s(.+)/gm, '<h3>$1</h3>')
      .replace(/^##\s(.+)/gm, '<h2>$1</h2>')
      .replace(/^#\s(.+)/gm, '<h1>$1</h1>')
      .replace(/^\s*[-*]\s(.+)/gm, '<li>$1</li>')
      .replace(/(<li>.*<\/li>)/gm, '<ul>$1</ul>')
      .replace(/\[(.*?)\]\((.*?)\)/g, '<a href="$2">$1</a>')
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/__(.*?)__/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      .replace(/_(.*?)_/g, '<em>$1</em>')
      .replace(/!\[.*?\]\(.*?\)/g, '')
      .replace(/```([\s\S]*?)```/g, '<pre><code>$1</code></pre>')
      .replace(/`([^`]+)`/g, '<code>$1</code>')
      .replace(/^(.+)\n?/gm, '<p>$1</p>');
  }

  // DOM Elements
  const chartElement = document.getElementById(chartElementId);
  const responseDiv = document.getElementById(responseDivId);

  if (!chartElement) {
    console.error("Chart element not found!");
    return;
  }

  // Display Loading Indicator
  responseDiv.innerHTML = `
    <div id="loading" style="display: flex; justify-content: center; align-items: center; height: 10vh; text-align: center; font-size: 16px;">
      <div>
        <p>Uploading and analyzing chart...</p>
        <div class="spinner" style="width: 30px; height: 30px; border: 4px solid #ccc; border-top: 4px solid #007bff; border-radius: 50%; animation: spin 1s linear infinite;"></div>
      </div>
    </div>
  `;

  try {
    // Capture Chart as Canvas
    const canvas = await html2canvas(chartElement);
    const imageBlob = await new Promise(resolve => canvas.toBlob(resolve, "image/png"));

    // Prepare FormData
    const formData = new FormData();
    formData.append("UploaderName", "none");
    formData.append("UploaderAddress", "Analyzer");
    formData.append("File", imageBlob);

    // Send POST Request
    const response = await fetch(apiUrl, {
      method: "POST",
      body: formData
    });

    if (response.ok) {
      const res = await response.json();
      const analysisText = res?.analysis || '';
      const result = markdownToHtml(analysisText);

      // Update Response Div with Analysis and Buttons
      responseDiv.innerHTML = `
        <div class="alert text-black alert-dismissible fade show" role="alert">
          <strong>Analysis Status:</strong> Success<br>
          <strong>Analysis:</strong> ${result}
          <div class="mt-3">
            <button class="btn btn-outline-primary btn-sm" id="copyBtn">
              <i class='bx bx-copy'></i> Copy
            </button>
            <button class="btn btn-outline-success btn-sm" id="exportBtn">
              <i class='bx bx-download'></i> Export
            </button>
            <button class="btn btn-outline-secondary btn-sm" id="printBtn">
              <i class='bx bx-printer'></i> Print
            </button>
          </div>
          <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
      `;

      // Attach Event Listeners to Buttons
      document.getElementById('copyBtn').addEventListener('click', () => copyToClipboard(analysisText));
      document.getElementById('exportBtn').addEventListener('click', () => exportToFile(analysisText));
      document.getElementById('printBtn').addEventListener('click', () => printAnalysis(analysisText));
    } else {
      responseDiv.innerHTML = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
          <strong>Analysis Status:</strong> Failed<br>
          <strong>Error:</strong> ${response.statusText}
          <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
      `;
    }
  } catch (error) {
    responseDiv.innerHTML = `
      <div class="alert alert-danger alert-dismissible fade show" role="alert">
        <strong>Analysis Status:</strong> Error<br>
        <strong>Error Details:</strong> ${error.message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
      </div>
    `;
  }
}
