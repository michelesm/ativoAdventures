document.addEventListener('DOMContentLoaded', function () {
    // Obesity Chart
    const obesityChartEl = document.getElementById('obesityChart');
    if (obesityChartEl) {
        const ctx = obesityChartEl.getContext('2d');
        const obesityData = {
            labels: ['2020', '2035 (Projeção)'],
            datasets: [{
                label: 'Jovens com IMC Elevado (em milhões)',
                data: [15, 20],
                backgroundColor: [
                    '#4ECDC4',
                    '#FF6B6B'
                ],
                borderColor: [
                    '#45B7D1',
                    '#ff4141'
                ],
                borderWidth: 2,
                borderRadius: 8,
                barThickness: 50,
            }]
        };
        const chartOptions = {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    enabled: true,
                    backgroundColor: '#333333',
                    titleFont: { size: 16, weight: 'bold' },
                    bodyFont: { size: 14 },
                    padding: 12,
                    cornerRadius: 8,
                    callbacks: {
                        title: function (tooltipItems) {
                            const item = tooltipItems[0];
                            let label = item.chart.data.labels[item.dataIndex];
                            if (Array.isArray(label)) {
                                return label.join(' ');
                            } else {
                                return label;
                            }
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'Número de Jovens (em milhões)',
                        font: {
                            size: 14,
                            weight: 'bold'
                        },
                        color: '#555'
                    },
                    grid: {
                        color: '#e0e0e0',
                        borderDash: [5, 5],
                    }
                },
                x: {
                    grid: {
                        display: false,
                    }
                }
            }
        };
        new Chart(ctx, {
            type: 'bar',
            data: obesityData,
            options: chartOptions
        });
    }

    // Inactivity Chart
    const inactivityChartEl = document.getElementById('inactivityChart');
    if (inactivityChartEl) {
        const ctx = inactivityChartEl.getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Jovens Fisicamente Inativos', 'Jovens Fisicamente Ativos'],
                datasets: [{
                    label: '% de Jovens',
                    data: [84, 16],
                    backgroundColor: [
                        'rgba(239, 68, 68, 0.6)',
                        'rgba(16, 185, 129, 0.6)'
                    ],
                    borderColor: [
                        'rgba(239, 68, 68, 1)',
                        'rgba(16, 185, 129, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                indexAxis: 'y',
                scales: {
                    x: {
                        beginAtZero: true,
                        max: 100,
                        ticks: {
                            color: '#6b7280',
                            callback: function (value) {
                                return value + '%'
                            }
                        }
                    },
                    y: {
                        ticks: {
                            color: '#374151',
                            font: {
                                weight: 'bold'
                            }
                        }
                    }
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        callbacks: {
                            title: function (tooltipItems) {
                                const item = tooltipItems[0];
                                let label = item.chart.data.labels[item.dataIndex];
                                if (Array.isArray(label)) {
                                    return label.join(' ');
                                } else {
                                    return label;
                                }
                            },
                            label: function (context) {
                                return context.dataset.label + ': ' + context.raw + '%';
                            }
                        }
                    }
                }
            }
        });
    }

    // Tabs
    const tabs = document.querySelectorAll('.tab-button');
    const tabContents = document.querySelectorAll('.tab-content');
    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(item => item.classList.remove('active'));
            tab.classList.add('active');
            const target = document.getElementById(tab.dataset.tab);
            tabContents.forEach(content => {
                content.classList.add('hidden', 'opacity-0');
            });
            target.classList.remove('hidden');
            setTimeout(() => target.classList.remove('opacity-0'), 50);
        });
    });

    // Mobile menu
    const mobileMenuButton = document.getElementById('mobile-menu-button');
    const mobileMenu = document.getElementById('mobile-menu');
    if (mobileMenuButton && mobileMenu) {
        mobileMenuButton.addEventListener('click', () => {
            mobileMenu.classList.toggle('hidden');
        });
    }

    // Smooth scroll and close mobile menu
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            if (mobileMenu && mobileMenu.classList.contains('hidden') === false) {
                mobileMenu.classList.add('hidden');
            }
            const targetElement = document.querySelector(this.getAttribute('href'));
            if (targetElement) {
                targetElement.scrollIntoView({
                    behavior: 'smooth'
                });
            }
        });
    });

    // Botão para carregar o conteúdo dinâmico
    const btn = document.getElementById('descubraProjetoBtn');
    if (btn) {
        btn.addEventListener('click', function (e) {
            e.preventDefault();
            fetch('conteudo.html')
                .then(response => {
                    if (!response.ok) throw new Error('Erro ao carregar conteúdo');
                    return response.text();
                })
                .then(html => {
                    // Esconde a section hero
                    const hero = document.getElementById('hero');
                    if (hero) hero.style.display = 'none';
                    // Insere o conteúdo carregado
                    document.getElementById('conteudo-dinamico').innerHTML = html;
                    window.scrollTo({ top: 0, behavior: 'smooth' });
                    // Se precisar inicializar gráficos ou outros scripts, chame aqui
                    if (typeof inicializarGraficos === 'function') inicializarGraficos();
                    if (typeof inicializarTabs === 'function') inicializarTabs();
                    if (typeof inicializarMenuMobile === 'function') inicializarMenuMobile();
                })
                .catch(err => {
                    alert('Não foi possível carregar o conteúdo.');
                });
        });
    }

    // Funções de inicialização para gráficos, tabs, menu mobile etc.
    window.inicializarGraficos = function () {
        // ... (coloque aqui o código dos gráficos Chart.js)
    };
    window.inicializarTabs = function () {
        // ... (coloque aqui o código dos tabs, se houver)
    };
    window.inicializarMenuMobile = function () {
        // ... (coloque aqui o código do menu mobile, se houver)
    };

    // Se quiser que funcione também ao abrir direto a página completa:
    if (document.getElementById('obesityChart')) window.inicializarGraficos();
    if (document.querySelector('.tab-button')) window.inicializarTabs();
    if (document.getElementById('mobile-menu-button')) window.inicializarMenuMobile();
});