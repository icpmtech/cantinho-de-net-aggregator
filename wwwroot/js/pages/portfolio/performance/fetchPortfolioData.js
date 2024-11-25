// Function to fetch portfolio data

export async function fetchPortfolioData() {
    try {
        const response = await fetch('/api/Portfolio');
        if (!response.ok) {
            throw new Error(`Error fetching data: ${response.statusText}`);
        }
        return await response.json();
    } catch (error) {
        console.error('Error:', error);
        throw error;
    }
}
