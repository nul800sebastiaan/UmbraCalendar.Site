document.addEventListener('DOMContentLoaded', function() {
    const timeElements = document.querySelectorAll('.fromTo');
    
    timeElements.forEach(element => {
        element.style.cursor = 'pointer';
        element.title = 'Click to see time conversion';
        
        element.addEventListener('click', function(event) {
            event.preventDefault();
            event.stopPropagation();
            
            console.log('Time element clicked!');
            
            const startTimeISO = this.getAttribute('data-isoISO8601-time-start');
            const endTimeISO = this.getAttribute('data-isoISO8601-time-end');
            
            console.log('Start time:', startTimeISO);
            console.log('End time:', endTimeISO);
            
            if (startTimeISO && endTimeISO) {
                const startDate = new Date(startTimeISO);
                const endDate = new Date(endTimeISO);
                
                // Get original time and timezone from the element's text
                const originalTimeText = this.textContent.split(' — ')[0];
                const originalTimeZone = this.textContent.split(' — ')[1] || 'UTC';
                
                // Get local time
                const localStartTime = startDate.toLocaleTimeString([], { 
                    hour: '2-digit', 
                    minute: '2-digit',
                    hour12: false 
                });
                
                const localEndTime = endDate.toLocaleTimeString([], { 
                    hour: '2-digit', 
                    minute: '2-digit',
                    hour12: false 
                });
                
                const localTimeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
                
                // Create popup content
                const popupContent = `
                    <strong>Original Time:</strong><br>
                    ${originalTimeText} (${originalTimeZone})<br><br>
                    <strong>Your Local Time:</strong><br>
                    ${localStartTime}-${localEndTime} (${localTimeZone})
                `;
                
                showTimePopup(event, popupContent);
            }
        });
    });
    
    function showTimePopup(event, content) {
        // Remove any existing popup
        const existingPopup = document.querySelector('.time-popup');
        if (existingPopup) {
            existingPopup.remove();
        }
        
        // Create popup element
        const popup = document.createElement('div');
        popup.className = 'time-popup';
        popup.innerHTML = content;
        
        // Style the popup
        popup.style.cssText = `
            position: absolute;
            background: white;
            border: 2px solid #333;
            border-radius: 8px;
            padding: 12px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.3);
            font-size: 14px;
            line-height: 1.4;
            max-width: 300px;
            z-index: 1000;
            color: #333;
        `;
        
        // Position popup near the click
        document.body.appendChild(popup);
        
        const rect = popup.getBoundingClientRect();
        const x = event.pageX;
        const y = event.pageY;
        
        // Adjust position to keep popup on screen
        let left = x + 10;
        let top = y - rect.height - 10;
        
        if (left + rect.width > window.innerWidth) {
            left = x - rect.width - 10;
        }
        if (top < window.pageYOffset) {
            top = y + 10;
        }
        
        popup.style.left = left + 'px';
        popup.style.top = top + 'px';
        
        // Close popup when clicking outside
        setTimeout(() => {
            document.addEventListener('click', function closePopup(e) {
                if (!popup.contains(e.target)) {
                    popup.remove();
                    document.removeEventListener('click', closePopup);
                }
            });
        }, 100);
        
        // Close popup after 5 seconds
        setTimeout(() => {
            if (popup && popup.parentNode) {
                popup.remove();
            }
        }, 5000);
    }
});