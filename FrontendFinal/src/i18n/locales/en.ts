import { TranslationKeys } from './es';

export const en: TranslationKeys = {
    // Navegación
    nav: {
        home: 'Home',
        explore: 'Explore',
        dashboard: 'Dashboard',
        events: 'Events',
        myTickets: 'My Tickets',
        profile: 'Profile',
        admin: 'Administration',
        logout: 'Logout',
        login: 'Login',
        register: 'Register',
        connected: 'Connected'
    },

    // Admin Menu
    adminMenu: {
        dashboard: 'Dashboard',
        events: 'Events',
        sales: 'Sales',
        finance: 'Finance',
        audit: 'Audit',
        supervision: 'Supervision',
        logs: 'Logs',
        users: 'Users'
    },

    // Común
    common: {
        search: 'Search',
        filter: 'Filter',
        export: 'Export',
        refresh: 'Refresh',
        save: 'Save',
        cancel: 'Cancel',
        delete: 'Delete',
        edit: 'Edit',
        create: 'Create',
        view: 'View',
        back: 'Back',
        next: 'Next',
        previous: 'Previous',
        loading: 'Loading',
        noResults: 'No results',
        error: 'Error',
        success: 'Success',
        warning: 'Warning',
        info: 'Information',
        confirm: 'Confirm',
        close: 'Close',
        select: 'Select',
        all: 'All',
        none: 'None',
        yes: 'Yes',
        no: 'No',
        of: 'of',
        to: 'To',
        type: 'Type'
    },

    // Sistema
    system: {
        signalrConnected: 'SignalR: Connected',
        signalrDisconnected: 'SignalR: Disconnected'
    },

    // Eventos
    events: {
        title: 'Events',
        event: 'Event',
        upcoming: 'Upcoming Events',
        past: 'Past Events',
        virtual: 'Virtual Event',
        inPerson: 'In-Person Event',
        date: 'Date',
        location: 'Location',
        price: 'Price',
        available: 'Available',
        soldOut: 'Sold Out',
        buyTicket: 'Buy Ticket',
        details: 'Details',
        description: 'Description',
        category: 'Category',
        organizer: 'Organizer'
    },

    // Entradas
    tickets: {
        title: 'My Tickets',
        myTickets: 'My Tickets',
        ticketCode: 'Ticket Code',
        status: 'Status',
        paid: 'Paid',
        pending: 'Pending',
        cancelled: 'Cancelled',
        used: 'Used',
        download: 'Download',
        qrCode: 'QR Code',
        seat: 'Seat',
        event: 'Event',
        purchaseDate: 'Purchase Date',
        totalPrice: 'Total Price'
    },

    // Perfil
    profile: {
        title: 'My Profile',
        personalInfo: 'Personal Information',
        name: 'Name',
        email: 'Email',
        phone: 'Phone',
        address: 'Address',
        editProfile: 'Edit Profile',
        changePassword: 'Change Password',
        currentPassword: 'Current Password',
        newPassword: 'New Password',
        confirmPassword: 'Confirm Password',
        settings: 'Settings',
        history: 'History',
        emails: 'Emails'
    },

    // Dashboard Admin
    dashboard: {
        title: 'Dashboard',
        welcome: 'Welcome',
        overview: 'Overview',
        statistics: 'Statistics',
        recentActivity: 'Recent Activity',
        quickActions: 'Quick Actions',
        totalSales: 'Total Sales',
        totalRevenue: 'Total Revenue',
        activeEvents: 'Active Events',
        totalUsers: 'Total Users',
        analysis: 'Business Analysis',
        description: 'Consolidated view of sales and event occupancy.',
        last7Days: 'LAST 7 DAYS',
        ticketsSold: 'Tickets Sold',
        eventsWithData: 'Events with Data',
        ticketingRate: 'Ticketing Rate',
        avgTicketDescription: 'Average sale per ticket',
        revenueEvolution: 'Revenue Evolution',
        dailySalesVolume: 'Daily sales volume for the period',
        occupancy: 'Occupancy',
        inventoryStatus: 'Seat inventory status',
        organizerMode: 'Organizer Mode Active',
        organizerMetricsDescription: 'The metrics shown correspond only to your assigned events.'
    },

    // Reportes
    reports: {
        title: 'Sales Reports',
        salesReport: 'Sales Report',
        revenue: 'Revenue',
        tickets: 'Tickets',
        averageTicket: 'Average Ticket',
        today: 'Today',
        thisWeek: 'This Week',
        thisMonth: 'This Month',
        topEvents: 'Top Events',
        salesByDay: 'Sales by Day',
        salesByHour: 'Sales by Hour'
    },

    // Supervisión
    supervision: {
        title: 'Technical Supervision',
        systemHealth: 'System Health',
        services: 'Services',
        active: 'Active',
        degraded: 'Degraded',
        down: 'Down',
        healthy: 'Healthy',
        responseTime: 'Response Time',
        uptime: 'Uptime',
        version: 'Version',
        port: 'Port',
        cpu: 'CPU',
        memory: 'Memory',
        requests: 'Requests'
    },

    // Logs
    logs: {
        title: 'Log Viewer',
        terminal: 'Log Terminal',
        level: 'Level',
        service: 'Service',
        message: 'Message',
        timestamp: 'Timestamp',
        details: 'Details',
        streaming: 'Streaming',
        autoScroll: 'Auto-scroll',
        clear: 'Clear',
        debug: 'Debug',
        info: 'Info',
        warning: 'Warning',
        error: 'Error',
        critical: 'Critical',
        stackTrace: 'Stack Trace'
    },

    // Finanzas
    finance: {
        title: 'Financial Reconciliation',
        description: 'Complete dashboard for transactions, income and financial metrics of the system.',
        totalIncome: 'Total Income',
        netIncome: 'Net Income',
        transactions: 'Transactions',
        approved: 'Approved',
        rejected: 'Rejected',
        pending: 'Pending',
        refunded: 'Refunded',
        approvalRate: 'Approval Rate',
        transactionDetails: 'Transaction Details',
        card: 'Card',
        amount: 'Amount',
        order: 'Order'
    },

    // Auditoría
    audit: {
        title: 'System Audit',
        userActions: 'User Actions',
        systemLogs: 'System Logs',
        action: 'Action',
        user: 'User',
        date: 'Date',
        result: 'Result',
        successful: 'Successful',
        failed: 'Failed',
        purchase: 'Purchase',
        payment: 'Payment',
        cancellation: 'Cancellation',
        usage: 'Usage'
    },

    // Correos
    emails: {
        title: 'Email History',
        emailHistory: 'Email History',
        subject: 'Subject',
        recipient: 'Recipient',
        type: 'Type',
        status: 'Status',
        sent: 'Sent',
        delivered: 'Delivered',
        content: 'Content',
        confirmation: 'Confirmation',
        reminder: 'Reminder',
        cancellation: 'Cancellation',
        refund: 'Refund',
        welcome: 'Welcome',
        promotion: 'Promotion'
    },

    // Mensajes
    messages: {
        loadingData: 'Loading data...',
        savingChanges: 'Saving changes...',
        deleteConfirm: 'Are you sure you want to delete this?',
        saveSuccess: 'Saved successfully',
        saveError: 'Error saving',
        deleteSuccess: 'Deleted successfully',
        deleteError: 'Error deleting',
        loginRequired: 'You must log in',
        unauthorized: 'Unauthorized',
        notFound: 'Not found',
        serverError: 'Server error',
        networkError: 'Network error'
    },

    // Fechas
    dates: {
        today: 'Today',
        yesterday: 'Yesterday',
        tomorrow: 'Tomorrow',
        thisWeek: 'This week',
        lastWeek: 'Last week',
        thisMonth: 'This month',
        lastMonth: 'Last month',
        thisYear: 'This year',
        days: 'days',
        hours: 'hours',
        minutes: 'minutes',
        seconds: 'seconds'
    },

    // Admin Eventos
    adminEvents: {
        title: 'Event Management',
        description: 'Create, edit and organize all your events from here.',
        createNew: 'Create New Event',
        searchPlaceholder: 'Search by title or place...',
        filters: 'Filters',
        table: {
            event: 'Event',
            dateTime: 'Date and Time',
            location: 'Location',
            status: 'Status',
            actions: 'Actions'
        },
        status: {
            published: 'Published',
            draft: 'Draft',
            unknown: 'Unknown'
        },
        dialogs: {
            deleteConfirm: 'Are you sure you want to delete this event? This action cannot be undone.',
            publishConfirm: 'Do you want to publish this event? Once published it will be visible to all users.'
        },
        messages: {
            loading: 'Loading catalog...',
            noResults: 'No events were found with the search criteria.',
            deleteError: 'Could not delete the event',
            publishError: 'Could not publish the event'
        },
        buttons: {
            publish: 'Publish',
            coupons: 'Coupons'
        }
    },

    // Footer
    footer: {
        rights: 'All rights reserved.',
        documentation: 'Documentation',
        support: 'Technical Support',
        systemStatus: 'System Status',
        privacy: 'Privacy',
        terms: 'Terms',
        supportLink: 'Support'
    },

    // Home
    home: {
        exclusiveExperiences: 'Exclusive Experiences',
        discoverNextEvent: 'DISCOVER YOUR NEXT EVENT',
        heroDescription: 'Explore the most premium selection of concerts, sports and festivals. Your ticket to the world of entertainment starts here.',
        syncingCatalog: 'Syncing Catalog...',
        totalSilence: 'TOTAL SILENCE...',
        noActiveEvents: 'No active events at this time.',
        viewTickets: 'View Tickets and Prices'
    },

    // Admin Usuarios
    adminUsers: {
        title: 'User Management',
        description: 'Manage users and organizers of the system',
        createNew: 'Create User',
        totalUsers: 'Total Users',
        organizers: 'Organizers',
        admins: 'Administrators',
        table: {
            user: 'User',
            email: 'Email',
            role: 'Role',
            created: 'Creation Date',
            actions: 'Actions'
        },
        roles: {
            admin: 'Administrator',
            organizer: 'Organizer',
            user: 'User'
        },
        modal: {
            title: 'Create New User',
            description: 'Register a new organizer or administrator',
            username: 'Username',
            fullName: 'Full Name',
            phone: 'Phone',
            address: 'Address',
            password: 'Password',
            role: 'Role',
            creating: 'Creating...',
            success: 'User created successfully',
            error: 'Error creating user'
        },
        delete: {
            confirm: 'Are you sure you want to delete user "{username}"?',
            success: 'User deleted successfully',
            error: 'Error deleting user'
        },
        validation: {
            usernameRequired: 'Username is required',
            usernameMin: 'Minimum 3 characters',
            emailRequired: 'Email is required',
            emailInvalid: 'Invalid email',
            nameRequired: 'Name is required',
            phoneRequired: 'Phone is required',
            phoneInvalid: 'Invalid phone (10 digits)',
            addressRequired: 'Address is required',
            passwordRequired: 'Password is required',
            passwordMin: 'Minimum 8 characters'
        }
    }
};
