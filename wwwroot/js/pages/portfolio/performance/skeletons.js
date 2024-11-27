// skeletons.js

// Function to show all skeleton placeholders
export function showSkeletons() {
  console.log('Showing skeletons');
  const skeletons = document.querySelectorAll('.skeleton');
  skeletons.forEach(skeleton => skeleton.classList.remove('d-none'));

  const contents = document.querySelectorAll('.content');
  contents.forEach(content => content.classList.add('d-none'));
}

// Function to hide all skeleton placeholders
export function hideSkeletons() {
  console.log('Hiding skeletons');
  const skeletons = document.querySelectorAll('.skeleton');
  skeletons.forEach(skeleton => skeleton.classList.add('d-none'));

  const contents = document.querySelectorAll('.content');
  contents.forEach(content => content.classList.remove('d-none'));
}
