// notification.js - SignalR client for real-time notifications

"use strict";

// Create connection to the SignalR hub
var connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .withAutomaticReconnect([0, 2000, 5000, 10000, 15000, 30000]) // Reconnect pattern in milliseconds
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start connection
function startConnection() {
    connection.start()
        .then(function () {
            console.log("SignalR Connected");

            // Get initial unread notifications
            connection.invoke("GetUnreadNotifications").catch(function (err) {
                console.error(err.toString());
            });

            // Set connection status indicator
            document.getElementById("connectionStatus").classList.remove("bg-danger");
            document.getElementById("connectionStatus").classList.add("bg-success");
        })
        .catch(function (err) {
            console.error(err.toString());
            document.getElementById("connectionStatus").classList.remove("bg-success");
            document.getElementById("connectionStatus").classList.add("bg-danger");

            // Retry connection after 5 seconds
            setTimeout(startConnection, 5000);
        });
}

// Connection closed event
connection.onclose(function () {
    console.log("SignalR Disconnected");
    document.getElementById("connectionStatus").classList.remove("bg-success");
    document.getElementById("connectionStatus").classList.add("bg-danger");

    // Retry connection after 5 seconds
    setTimeout(startConnection, 5000);
});

// Receive new notification
connection.on("ReceiveNotification", function (notification) {
    // Add notification to the list
    addNotificationToList(notification);

    // Show toast notification
    showToast(notification);

    // Play notification sound
    playNotificationSound();
});

// Receive unread count
connection.on("ReceiveUnreadCount", function (count) {
    updateNotificationBadge(count);
});

// Receive unread notifications list
connection.on("ReceiveUnreadNotifications", function (notifications) {
    // Clear existing notifications
    var notificationList = document.getElementById("notificationList");
    notificationList.innerHTML = "";

    // Add each notification to the list
    notifications.forEach(function (notification) {
        addNotificationToList(notification);
    });

    // Update empty state message
    if (notifications.length === 0) {
        notificationList.innerHTML = '<li class="dropdown-item text-center text-muted">No unread notifications</li>';
    }
});

// Add notification to list
function addNotificationToList(notification) {
    var notificationList = document.getElementById("notificationList");

    // Check if empty state message exists and remove it
    if (notificationList.innerHTML.includes("No unread notifications")) {
        notificationList.innerHTML = "";
    }

    // Create notification item
    var li = document.createElement("li");
    li.className = "dropdown-item notification-item";
    li.setAttribute("data-notification-id", notification.id);

    // Set background color based on type
    var bgColor = "";
    var icon = "";

    switch (notification.type) {
        case "Approval":
            bgColor = "border-left border-success";
            icon = '<i class="fas fa-check-circle text-success me-2"></i>';
            break;
        case "Rejection":
            bgColor = "border-left border-danger";
            icon = '<i class="fas fa-times-circle text-danger me-2"></i>';
            break;
        case "PendingApproval":
            bgColor = "border-left border-warning";
            icon = '<i class="fas fa-clock text-warning me-2"></i>';
            break;
        default:
            bgColor = "border-left border-info";
            icon = '<i class="fas fa-info-circle text-info me-2"></i>';
    }

    // Create notification content
    var content = `
        <div class="d-flex ${bgColor}">
            <div class="ms-2">
                ${icon}
            </div>
            <div class="flex-grow-1">
                <h6 class="mb-1">${notification.title}</h6>
                <p class="mb-1 small">${notification.message}</p>
                <small class="text-muted">${moment(notification.createdAt).fromNow()}</small>
            </div>
            <div class="ms-auto">
                <button class="btn btn-sm btn-link text-muted mark-as-read" data-notification-id="${notification.id}">
                    <i class="fas fa-check"></i>
                </button>
            </div>
        </div>
    `;

    li.innerHTML = content;

    // Add event listener for marking as read
    li.querySelector(".mark-as-read").addEventListener("click", function (e) {
        e.preventDefault();
        e.stopPropagation();
        markAsRead(notification.id);
    });

    // Add event listener for clicking on the notification
    li.addEventListener("click", function () {
        if (notification.url) {
            markAsRead(notification.id);
            window.location.href = notification.url;
        }
    });

    // Add to notification list (at the top)
    notificationList.insertBefore(li, notificationList.firstChild);
}

// Mark notification as read
function markAsRead(notificationId) {
    connection.invoke("MarkAsRead", notificationId).catch(function (err) {
        console.error(err.toString());
    });

    // Remove from list
    var item = document.querySelector(`.notification-item[data-notification-id="${notificationId}"]`);
    if (item) {
        item.remove();
    }

    // Check if list is empty
    var notificationList = document.getElementById("notificationList");
    if (notificationList.childElementCount === 0) {
        notificationList.innerHTML = '<li class="dropdown-item text-center text-muted">No unread notifications</li>';
    }
}

// Mark all notifications as read
function markAllAsRead() {
    fetch('/Notification/MarkAllAsRead', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'X-CSRF-TOKEN': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    })
        .then(response => {
            if (response.ok) {
                // Clear notification list
                var notificationList = document.getElementById("notificationList");
                notificationList.innerHTML = '<li class="dropdown-item text-center text-muted">No unread notifications</li>';

                // Update badge
                updateNotificationBadge(0);
            }
        })
        .catch(error => console.error('Error:', error));
}

// Update notification badge
function updateNotificationBadge(count) {
    var badge = document.getElementById("notificationBadge");

    if (count > 0) {
        badge.textContent = count > 99 ? "99+" : count;
        badge.classList.remove("d-none");
    } else {
        badge.textContent = "";
        badge.classList.add("d-none");
    }
}

// Show toast notification
function showToast(notification) {
    // Create toast container if it doesn't exist
    var toastContainer = document.getElementById("toastContainer");
    if (!toastContainer) {
        toastContainer = document.createElement("div");
        toastContainer.id = "toastContainer";
        toastContainer.className = "toast-container position-fixed bottom-0 end-0 p-3";
        document.body.appendChild(toastContainer);
    }

    // Set icon based on notification type
    var icon = "";
    var bgClass = "";

    switch (notification.type) {
        case "Approval":
            icon = '<i class="fas fa-check-circle me-2"></i>';
            bgClass = "bg-success text-white";
            break;
        case "Rejection":
            icon = '<i class="fas fa-times-circle me-2"></i>';
            bgClass = "bg-danger text-white";
            break;
        case "PendingApproval":
            icon = '<i class="fas fa-clock me-2"></i>';
            bgClass = "bg-warning";
            break;
        default:
            icon = '<i class="fas fa-info-circle me-2"></i>';
            bgClass = "bg-info text-white";
    }

    // Create toast element
    var toastId = "toast-" + Date.now();
    var toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="5000">
            <div class="toast-header ${bgClass}">
                ${icon}
                <strong class="me-auto">${notification.title}</strong>
                <small>${moment(notification.createdAt).fromNow()}</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
            <div class="toast-body">
                ${notification.message}
            </div>
        </div>
    `;

    // Add toast to container
    toastContainer.innerHTML += toastHtml;

    // Initialize and show toast
    var toastElement = document.getElementById(toastId);
    var toast = new bootstrap.Toast(toastElement);
    toast.show();

    // Add click event to navigate to URL
    if (notification.url) {
        toastElement.addEventListener("click", function () {
            markAsRead(notification.id);
            window.location.href = notification.url;
        });
        toastElement.style.cursor = "pointer";
    }

    // Remove toast element after it's hidden
    toastElement.addEventListener('hidden.bs.toast', function () {
        toastElement.remove();
    });
}

// Play notification sound
function playNotificationSound() {
    var audio = new Audio('/sounds/notification.mp3');
    audio.play();
}

// Document ready
document.addEventListener("DOMContentLoaded", function () {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Mark all as read button
    var markAllReadBtn = document.getElementById("markAllAsRead");
    if (markAllReadBtn) {
        markAllReadBtn.addEventListener("click", function (e) {
            e.preventDefault();
            markAllAsRead();
        });
    }

    // Start SignalR connection
    startConnection();
});