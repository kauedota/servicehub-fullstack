// Ilustração vetorial (SVG) do próprio sistema: uma "tela" com indicadores, gráfico e lista de chamados.
// Usa branco com transparência, então se mistura com qualquer fundo escuro/colorido do banner
// e escala sem perder qualidade em qualquer tamanho de tela.
export default function HeroIlustracao() {
  const vidro = 'rgba(255,255,255,0.10)';
  const borda = 'rgba(255,255,255,0.28)';
  const linha = 'rgba(255,255,255,0.35)';
  const forte = 'rgba(255,255,255,0.85)';

  return (
    <svg className="hero-ilustracao" viewBox="0 0 480 360" role="img" aria-label="Ilustração do painel do Service Hub com indicadores, gráfico e chamados">
      {/* Janela principal */}
      <rect x="30" y="24" width="400" height="280" rx="16" fill={vidro} stroke={borda} />
      <circle cx="54" cy="46" r="5" fill="rgba(255,255,255,0.45)" />
      <circle cx="70" cy="46" r="5" fill="rgba(255,255,255,0.3)" />
      <circle cx="86" cy="46" r="5" fill="rgba(255,255,255,0.2)" />
      <line x1="30" y1="64" x2="430" y2="64" stroke={borda} />

      {/* Menu lateral */}
      <rect x="46" y="80" width="70" height="10" rx="5" fill={forte} />
      <rect x="46" y="102" width="56" height="8" rx="4" fill={linha} />
      <rect x="46" y="120" width="62" height="8" rx="4" fill={linha} />
      <rect x="46" y="138" width="50" height="8" rx="4" fill={linha} />
      <rect x="46" y="156" width="58" height="8" rx="4" fill={linha} />

      {/* Cards de indicadores */}
      {[132, 226, 320].map((x, i) => (
        <g key={x}>
          <rect x={x} y="80" width="84" height="54" rx="10" fill="rgba(255,255,255,0.14)" stroke={borda} />
          <rect x={x + 12} y="92" width="40" height="7" rx="3.5" fill={linha} />
          <text x={x + 12} y="124" fill="#fff" fontSize="20" fontWeight="700" fontFamily="Segoe UI, sans-serif">
            {['12', '8', '27'][i]}
          </text>
        </g>
      ))}

      {/* Gráfico de barras */}
      <rect x="132" y="148" width="130" height="140" rx="10" fill="rgba(255,255,255,0.14)" stroke={borda} />
      {[46, 78, 58, 96, 70].map((h, i) => (
        <rect key={i} x={146 + i * 22} y={276 - h} width="14" height={h} rx="4" fill={i === 3 ? '#ffffff' : 'rgba(255,255,255,0.55)'} />
      ))}

      {/* Lista de chamados */}
      <rect x="274" y="148" width="130" height="140" rx="10" fill="rgba(255,255,255,0.14)" stroke={borda} />
      {[0, 1, 2, 3].map((i) => (
        <g key={i}>
          <circle cx="290" cy={172 + i * 32} r="6" fill={['#ff8a80', '#ffcc80', '#90caf9', '#a5d6a7'][i]} />
          <rect x="304" y={166 + i * 32} width="84" height="6" rx="3" fill={forte} />
          <rect x="304" y={176 + i * 32} width="56" height="5" rx="2.5" fill={linha} />
        </g>
      ))}

      {/* Card flutuante: chamado concluído */}
      <g className="hero-flutuante">
        <rect x="300" y="276" width="166" height="62" rx="12" fill="#ffffff" />
        <circle cx="326" cy="307" r="14" fill="#1d8a4a" />
        <path d="M319 307 l5 5 l9 -10" stroke="#fff" strokeWidth="3" fill="none" strokeLinecap="round" strokeLinejoin="round" />
        <text x="348" y="302" fill="#1c2333" fontSize="13" fontWeight="700" fontFamily="Segoe UI, sans-serif">Chamado #8</text>
        <text x="348" y="320" fill="#667085" fontSize="12" fontFamily="Segoe UI, sans-serif">Concluído</text>
      </g>

      {/* Bolha de atendimento */}
      <g>
        <circle cx="34" cy="292" r="28" fill="#ffffff" />
        <path d="M22 294 a12 12 0 0 1 24 0" stroke="#1f4fd8" strokeWidth="3.5" fill="none" strokeLinecap="round" />
        <rect x="18" y="292" width="7" height="12" rx="3" fill="#1f4fd8" />
        <rect x="43" y="292" width="7" height="12" rx="3" fill="#1f4fd8" />
      </g>
    </svg>
  );
}
