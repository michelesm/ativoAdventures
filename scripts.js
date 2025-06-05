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

    // Slides da section problema
    function inicializarSlidesProblema() {
        const slides = document.querySelectorAll('.problema-slide');
        const btnPrev = document.getElementById('problema-prev');
        const btnNext = document.getElementById('problema-next');
        if (!slides.length || !btnPrev || !btnNext) return;

        let atual = 0;
        function mostrarSlide(idx) {
            slides.forEach((slide, i) => {
                slide.classList.toggle('hidden', i !== idx);
            });
            btnPrev.disabled = idx === 0;
            btnNext.disabled = idx === slides.length - 1;
        }

        btnPrev.addEventListener('click', () => {
            if (atual > 0) {
                atual--;
                mostrarSlide(atual);
            }
        });
        btnNext.addEventListener('click', () => {
            if (atual < slides.length - 1) {
                atual++;
                mostrarSlide(atual);
            }
        });

        mostrarSlide(atual);
    }

    inicializarSlidesProblema();

    // Slides da section teoria
    (function inicializarSlidesTeoria() {
        const slides = document.querySelectorAll('.teoria-slide');
        const btnPrev = document.getElementById('teoria-prev');
        const btnNext = document.getElementById('teoria-next');
        if (!slides.length || !btnPrev || !btnNext) return;

        let atual = 0;

        function mostrarSlide(idx) {
            slides.forEach((slide, i) => {
                slide.classList.toggle('hidden', i !== idx);
            });
            btnPrev.disabled = idx === 0;
            btnNext.disabled = idx === slides.length - 1;
        }

        // Remove event listeners antigos (garante que só um listener será adicionado)
        btnPrev.onclick = null;
        btnNext.onclick = null;

        btnPrev.onclick = function () {
            if (atual > 0) {
                atual--;
                mostrarSlide(atual);
            }
        };
        btnNext.onclick = function () {
            if (atual < slides.length - 1) {
                atual++;
                mostrarSlide(atual);
            }
        };

        mostrarSlide(atual);
    })();


    // Octalysis Chart

    function wrapText(str) {
            const words = str.split(' ');
            const lines = [];
            let currentLine = words[0];

            for (let i = 1; i < words.length; i++) {
                if (currentLine.length + words[i].length + 1 < 16) {
                    currentLine += ' ' + words[i];
                } else {
                    lines.push(currentLine);
                    currentLine = words[i];
                }
            }
            lines.push(currentLine);
            return lines;
        }

        const octalysisLabelsRaw = [
            'Significado Épico',
            'Realização',
            'Empoderamento',
            'Propriedade e Posse',
            'Influência Social',
            'Escassez',
            'Imprevisibilidade',
            'Perda e Evitação'
        ];

        const octalysisLabels = octalysisLabelsRaw.map(label => label.length > 16 ? wrapText(label) : label);

        const octalysisData = {
            labels: octalysisLabels,
            datasets: [{
                label: 'Análise Inicial',
                data: [4, 5, 3, 2, 4, 1, 3, 2],
                backgroundColor: 'rgba(255, 107, 107, 0.2)',
                borderColor: '#FF6B6B',
                pointBackgroundColor: '#FF6B6B',
                pointBorderColor: '#fff',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: '#FF6B6B'
            }, {
                label: 'Estratégia Proposta',
                data: [8, 9, 7, 6, 7, 3, 6, 5],
                backgroundColor: 'rgba(78, 205, 196, 0.2)',
                borderColor: '#4ECDC4',
                pointBackgroundColor: '#4ECDC4',
                pointBorderColor: '#fff',
                pointHoverBackgroundColor: '#fff',
                pointHoverBorderColor: '#4ECDC4'
            }]
        };

        const chartConfig = {
            type: 'radar',
            data: octalysisData,
            options: {
                responsive: true,
                maintainAspectRatio: false,
                elements: {
                    line: {
                        borderWidth: 3
                    }
                },
                scales: {
                    r: {
                        angleLines: {
                            color: '#e0e0e0'
                        },
                        grid: {
                            color: '#e0e0e0'
                        },
                        pointLabels: {
                            font: {
                                size: 12,
                                weight: 'bold'
                            },
                             color: '#1A535C'
                        },
                        ticks: {
                            backdropColor: '#F7FFF7',
                            color: '#1A535C'
                        },
                        suggestedMin: 0,
                        suggestedMax: 10
                    }
                },
                plugins: {
                    tooltip: {
                        callbacks: {
                            title: function(tooltipItems) {
                                const item = tooltipItems[0];
                                let label = item.chart.data.labels[item.dataIndex];
                                if (Array.isArray(label)) {
                                  return label.join(' ');
                                } else {
                                  return label;
                                }
                            }
                        }
                    },
                    legend: {
                        position: 'top',
                    }
                }
            }
        };

        window.onload = function() {
            const ctx = document.getElementById('octalysisChart').getContext('2d');
            new Chart(ctx, chartConfig);
        };

    // Conteúdo dos cards de tecnologia
    const techDetails = {
        unity: {
            title: "Unity (com C#)",
            desc: "Unity é uma das engines de jogos mais populares do mundo, permitindo criar experiências 2D e 3D multiplataforma. Utiliza C# como linguagem principal para scripts e lógica do jogo.",
            img: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/unity/unity-original.svg"
        },
        health: {
            title: "Health Connect (com Plugin Java)",
            desc: "Health Connect é uma API do Google para integrar dados de saúde de diferentes apps. O plugin Java conecta o Unity ao Health Connect, permitindo que o jogo leia dados reais de atividade física.",
            img: "https://play-lh.googleusercontent.com/0n4Qw8w8vQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQwQw=w240-h480-rw"
        },
        linguagens: {
            title: "C# e Java",
            desc: "C# é usado para o desenvolvimento do jogo na Unity, enquanto Java é utilizado para criar o plugin que faz a ponte entre o jogo e o Health Connect no Android.",
            img: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/csharp/csharp-original.svg"
        },
        design: {
            title: "IA Generativa e Assets Livres",
            desc: "Para criar artes, sons e elementos visuais, foram utilizados recursos de IA generativa e bancos de assets livres, garantindo criatividade e respeito a direitos autorais.",
            img: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/photoshop/photoshop-plain.svg"
        },
        ide: {
            title: "VS Code e Android Studio",
            desc: "VS Code é um editor de código leve e versátil, ideal para scripts e organização do projeto. Android Studio é utilizado para compilar e testar o plugin Java no Android.",
            img: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/androidstudio/androidstudio-original.svg"
        },
        gestao: {
            title: "Notion e GitHub",
            desc: "Notion é usado para organização, planejamento e documentação do projeto. O GitHub gerencia o versionamento do código e colaboração entre desenvolvedores.",
            img: "https://cdn.jsdelivr.net/gh/devicons/devicon/icons/github/github-original.svg"
        }
    };

    // Interação dos cards de tecnologia
    const techCards = document.querySelectorAll('#tech-cards [data-tech]');
    const techDetailCard = document.getElementById('tech-detail-card');
    const techDetailContent = document.getElementById('tech-detail-content');
    const closeTechDetail = document.getElementById('close-tech-detail');

    techCards.forEach(card => {
        card.addEventListener('click', () => {
            const key = card.getAttribute('data-tech');
            const detail = techDetails[key];
            if (detail) {
                techDetailContent.innerHTML = `
                    <h3 class="text-xl font-bold mb-2 text-[#0077B6]">${detail.title}</h3>
                    <img src="${detail.img}" alt="${detail.title}" class="w-24 h-24 object-contain mx-auto mb-4">
                    <p class="text-gray-700 text-center">${detail.desc}</p>
                `;
                techDetailCard.classList.remove('hidden');
            }
        });
    });

    if (closeTechDetail) {
        closeTechDetail.addEventListener('click', () => {
            techDetailCard.classList.add('hidden');
        });
    }
    // Fecha ao clicar fora do card
    if (techDetailCard) {
        techDetailCard.addEventListener('click', (e) => {
            if (e.target === techDetailCard) {
                techDetailCard.classList.add('hidden');
            }
        });
    }
});