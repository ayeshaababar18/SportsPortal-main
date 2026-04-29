// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function() {
    // Navbar transparency effect
    const navbar = document.querySelector('.navbar');
    if (navbar) {
        function toggleNavbarTransparency() {
            if (window.scrollY > 50) {
                navbar.classList.remove('navbar-transparent');
            } else {
                navbar.classList.add('navbar-transparent');
            }
        }
        // Initial check
        toggleNavbarTransparency(); 
        // Event listener for scroll
        window.addEventListener('scroll', toggleNavbarTransparency);
    }

    // Scroll Progress Bar (SVG Border)
    const progressBarRect = document.getElementById('scroll-progress-bar');
    
    function updateProgress() {
        if (!progressBarRect) return;

        // Use documentElement for standards mode
        const elem = document.documentElement;
        
        const scrollTop = window.scrollY || elem.scrollTop;
        const scrollHeight = elem.scrollHeight - elem.clientHeight;
        
        // Avoid division by zero
        let scrollPercent = scrollHeight > 0 ? (scrollTop / scrollHeight) : 0;
        
        // Clamp strictly between 0 and 1
        scrollPercent = Math.max(0, Math.min(1, scrollPercent));
        
        const pathLength = progressBarRect.getTotalLength();
        
        // Set dasharray to the full length
        progressBarRect.style.strokeDasharray = pathLength;
        
        // Offset it by the remaining percentage
        const drawLength = pathLength * scrollPercent;
        progressBarRect.style.strokeDashoffset = pathLength - drawLength;
    }

    if (progressBarRect) {
        // Events to trigger update
        window.addEventListener('scroll', updateProgress, { passive: true });
        window.addEventListener('resize', updateProgress);
        
        // Observer for navbar size changes (width changes)
        const navObserver = new ResizeObserver(() => updateProgress());
        const navElement = document.querySelector('.floating-nav');
        if (navElement) navObserver.observe(navElement);
        
        // Observer for document height changes (content loading)
        const docObserver = new ResizeObserver(() => updateProgress());
        docObserver.observe(document.documentElement);
        
        // Initial call
        // Small delay to ensure layout is settled
        setTimeout(updateProgress, 100);
    }

    // Floating Nav & Layout Scroll Effect (Directional)
    const floatingNav = document.querySelector('.floating-nav');
    
    // Layout Elements for Collapse/Expand
    const leftSidebar = document.getElementById('sidebar-left-col');
    const rightSidebar = document.getElementById('sidebar-right-col');
    const mainContent = document.getElementById('main-content-col');

    let lastScrollTop = 0;
    
    // Layout Animation Listener
    window.addEventListener('scroll', function() {
        let st = window.pageYOffset || document.documentElement.scrollTop;
        
        // Collapse Sidebars Logic (Main content expands automatically via flex:1)
        if (leftSidebar && rightSidebar) {
            const shouldCollapse = st > 5;
            const isCollapsed = leftSidebar.classList.contains('sidebar-collapsed');

            if (shouldCollapse && !isCollapsed) {
                leftSidebar.classList.add('sidebar-collapsed');
                rightSidebar.classList.add('sidebar-collapsed');
            } else if (!shouldCollapse && isCollapsed) {
                leftSidebar.classList.remove('sidebar-collapsed');
                rightSidebar.classList.remove('sidebar-collapsed');
            }
        }
        
        // Floating Nav Logic
        if (floatingNav) {
            const scrollHeight = document.documentElement.scrollHeight;
            const clientHeight = document.documentElement.clientHeight;
            // Check if at bottom (with small buffer)
            const isAtBottom = (st + clientHeight) >= (scrollHeight - 10);

            if (st > lastScrollTop && st > 50) {
                // Scrolling Down
                floatingNav.classList.add('scrolled');
            } else if (st < lastScrollTop && !isAtBottom) {
                // Scrolling Up (but not bouncing at bottom)
                floatingNav.classList.remove('scrolled');
            }
        }
        
        lastScrollTop = st <= 0 ? 0 : st;
    }, false);

    // Back button functionality
    const backButton = document.getElementById('backButton');
    if (backButton) {
        backButton.addEventListener('click', function() {
            window.history.back();
        });
    }

    // Scroll Buttons Logic
    const scrollToTopBtn = document.getElementById('scrollToTopBtn');
    const scrollToBottomBtn = document.getElementById('scrollToBottomBtn');

    function toggleScrollButtons() {
        const scrollTop = window.pageYOffset || document.documentElement.scrollTop;
        const scrollable = document.documentElement.scrollHeight - window.innerHeight;

        if (scrollToTopBtn) {
             scrollToTopBtn.style.display = (scrollTop > 200) ? 'flex' : 'none'; // flex to center icon
        }
        if (scrollToBottomBtn) {
             scrollToBottomBtn.style.display = (scrollTop < scrollable - 50) ? 'flex' : 'none';
        }
    }

    // Initialize and listen
    if (scrollToTopBtn || scrollToBottomBtn) {
        window.addEventListener('scroll', toggleScrollButtons, { passive: true });
        toggleScrollButtons(); // Initial check
    }

    if (scrollToTopBtn) {
        scrollToTopBtn.addEventListener('click', function() {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    if (scrollToBottomBtn) {
        scrollToBottomBtn.addEventListener('click', function() {
            window.scrollTo({ top: document.documentElement.scrollHeight, behavior: 'smooth' });
        });
    }

    // Sidebar Updates (Live & Alerts)
    function updateSidebars() {
        // Fetch Live Matches
        fetch('/Home/GetLiveMatches')
            .then(response => response.json())
            .then(data => {
                const container = document.getElementById('live-updates-content');
                if (!container) return;
                
                if (data && data.length > 0) {
                    const liveMatch = data.find(m => m.status === 'Live');
                    if (liveMatch) {
                        container.innerHTML = `
                            <p class="small mb-1 fw-bold">${liveMatch.departmentA_Name} vs ${liveMatch.departmentB_Name}</p>
                            <p class="h6 mb-1">${liveMatch.scoreA} - ${liveMatch.scoreB}</p>
                            <span class="badge bg-danger pulse-animation">LIVE</span>
                            <small class="d-block mt-1">${liveMatch.sportName}</small>
                        `;
                    } else {
                        // Show next upcoming match
                        const nextMatch = data[0]; // Already ordered by date
                        container.innerHTML = `
                            <p class="small mb-1 text-uppercase opacity-75">Next Match</p>
                            <p class="small mb-1 fw-bold">${nextMatch.departmentA_Name} vs ${nextMatch.departmentB_Name}</p>
                            <small class="d-block">${nextMatch.timeRemaining}</small>
                            <small class="d-block opacity-75">${nextMatch.sportName}</small>
                        `;
                    }
                } else {
                     container.innerHTML = '<p class="small mb-0 opacity-75">No live or upcoming matches.</p>';
                }
            })
            .catch(err => console.error('Error fetching live matches:', err));

        // Fetch Alerts
        fetch('/Home/GetAlerts')
            .then(response => response.json())
            .then(data => {
                const container = document.getElementById('alerts-content');
                if (!container) return;

                if (data && data.length > 0) {
                    const latest = data[0];
                    container.innerHTML = `
                        <p class="small mb-1 fw-bold">${latest.priority === 'High' ? '<i class="bi bi-exclamation-circle-fill"></i> ' : ''}Announcement</p>
                        <p class="small mb-0 text-white" style="line-height: 1.2;">${latest.message}</p>
                        <small class="d-block mt-1 text-white-50" style="font-size: 0.7rem;">${latest.postedDate}</small>
                    `;
                } else {
                    container.innerHTML = '<p class="small text-white-50 mb-0">No new alerts.</p>';
                }
            })
            .catch(err => console.error('Error fetching alerts:', err));
    }

    // Run updates periodically
    if (document.getElementById('live-updates-content') || document.getElementById('alerts-content')) {
        updateSidebars(); // Initial call
        setInterval(updateSidebars, 30000); // Every 30s
    }
});