// import React from 'react';
// import styled from 'styled-components';

// const Button = () => {
//   return (
//     <StyledWrapper>
//       <div className="container-btn-pixel-art">
//         <input className="input" id="start" name="start" type="checkbox" />
//         <div className="text">
//           <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 27 5" fill="currentColor">
//             {/* Replace style={{-delay: n}} with style={{ '--delay': n } as React.CSSProperties } */}
//             <g className="pixels" style={{ '--delay': 1 } as React.CSSProperties}>
//               <path d="M1 1H0V2H1V1Z" />
//               <path d="M1 1H2V0H1V1Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 2 } as React.CSSProperties}>
//               <path d="M1 3V2H0V3H1Z" />
//               <path d="M1 2H2V1H1V2Z" />
//               <path d="M2 1H3V0H2V1Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 3 } as React.CSSProperties}>
//               <path d="M1 3H2V2H1V3Z" />
//               <path d="M3 0V1H4V0H3Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 4 } as React.CSSProperties}>
//               <path d="M1 5V4H0V5H1Z" />
//               <path d="M2 2V3H3V2H2Z" />
//               <path d="M4 1H5V0H4V1Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 5 } as React.CSSProperties}>
//               <path d="M2 4H1V5H2V4Z" />
//               <path d="M4 3V2H3V3H4Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 6 } as React.CSSProperties}>
//               <path d="M3 4H2V5H3V4Z" />
//               <path d="M4 3H3V4H4V3Z" />
//               <path d="M7 0H6V1H7V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 7 } as React.CSSProperties}>
//               <path d="M4 4H3V5H4V4Z" />
//               <path d="M5 3H4V4H5V3Z" />
//               <path d="M8 0H7V1H8V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 8 } as React.CSSProperties}>
//               <path d="M5 4H4V5H5V4Z" />
//               <path d="M8 1H7V2H8V1Z" />
//               <path d="M9 0H8V1H9V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 9 } as React.CSSProperties}>
//               <path d="M9 1H8V2H9V1Z" />
//               <path d="M10 0H9V1H10V0Z" />
//               <path d="M8 2H7V3H8V2Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 10 } as React.CSSProperties}>
//               <path d="M8 3H7V4H8V3Z" />
//               <path d="M9 2H8V3H9V2Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 11 } as React.CSSProperties}>
//               <path d="M8 4H7V5H8V4Z" />
//               <path d="M9 3H8V4H9V3Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 12 } as React.CSSProperties}>
//               <path d="M9 4H8V5H9V4Z" />
//               <path d="M12 1H11V2H12V1Z" />
//               <path d="M13 0H12V1H13V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 13 } as React.CSSProperties}>
//               <path d="M12 2H11V3H12V2Z" />
//               <path d="M13 1H12V2H13V1Z" />
//               <path d="M14 0H13V1H14V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 14 } as React.CSSProperties}>
//               <path d="M15 0H14V1H15V0Z" />
//               <path d="M13 2H12V3H13V2Z" />
//               <path d="M12 3H11V4H12V3Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 15 } as React.CSSProperties}>
//               <path d="M15 1H14V2H15V1Z" />
//               <path d="M14 2H13V3H14V2Z" />
//               <path d="M12 4H11V5H12V4Z" />
//               <path d="M13 3H12V4H13V3Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 16 } as React.CSSProperties}>
//               <path d="M12 5H13V4H12V5Z" />
//               <path d="M15 2H16V1H15V2Z" />
//               <path d="M14 3H15V2H14V3Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 17 } as React.CSSProperties}>
//               <path d="M15 3H14V4H15V3Z" />
//               <path d="M16 2H15V3H16V2Z" />
//               <path d="M18 0H17V1H18V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 18 } as React.CSSProperties}>
//               <path d="M15 4H14V5H15V4Z" />
//               <path d="M16 3H15V4H16V3Z" />
//               <path d="M18 1H17V2H18V1Z" />
//               <path d="M19 0H18V1H19V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 19 } as React.CSSProperties}>
//               <path d="M16 4H15V5H16V4Z" />
//               <path d="M18 2H17V3H18V2Z" />
//               <path d="M19 1H18V2H19V1Z" />
//               <path d="M20 0H19V1H20V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 20 } as React.CSSProperties}>
//               <path d="M21 0H20V1H21V0Z" />
//               <path d="M18 3H17V4H18V3Z" />
//               <path d="M19 2H18V3H19V2Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 21 } as React.CSSProperties}>
//               <path d="M21 1H20V2H21V1Z" />
//               <path d="M18 4H17V5H18V4Z" />
//               <path d="M19 3H18V4H19V3Z" />
//               <path d="M20 2H19V3H20V2Z" />
//               <path d="M22 0H21V1H22V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 22 } as React.CSSProperties}>
//               <path d="M18 5H19V4H18V5Z" />
//               <path d="M21 2H22V1H21V2Z" />
//               <path d="M20 3H21V2H20V3Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 23 } as React.CSSProperties}>
//               <path d="M21 3H20V4H21V3Z" />
//               <path d="M24 0H23V1H24V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 24 } as React.CSSProperties}>
//               <path d="M21 4H20V5H21V4Z" />
//               <path d="M22 3H21V4H22V3Z" />
//               <path d="M25 0H24V1H25V0Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 25 } as React.CSSProperties}>
//               <path d="M25 1H26V0H25V1Z" />
//               <path d="M24 2H25V1H24V2Z" />
//               <path d="M21 5H22V4H21V5Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 26 } as React.CSSProperties}>
//               <path d="M26 1H27V0H26V1Z" />
//               <path d="M25 2H26V1H25V2Z" />
//               <path d="M24 3H25V2H24V3Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 27 } as React.CSSProperties}>
//               <path d="M25 3H26V2H25V3Z" />
//               <path d="M24 4H25V3H24V4Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 28 } as React.CSSProperties}>
//               <path d="M25 4H26V3H25V4Z" />
//               <path d="M24 5H25V4H24V5Z" />
//             </g>
//             <g className="pixels" style={{ '--delay': 29 } as React.CSSProperties}>
//               <path d="M25 5H26V4H25V5Z" />
//             </g>
//           </svg>
//         </div>
//         {/* ...rest of your code remains unchanged... */}
//         <label className="button" htmlFor="start">
//           {/* ...SVG code... */}
//         </label>
//         <div className="link">
//           {/* ...SVG code... */}
//         </div>
//         <div className="light-up">
//           <span />
//         </div>
//       </div>
//     </StyledWrapper>
//   );
// }

// const StyledWrapper = styled.div`
//   /* ...existing styles... */
// `;

// export default Button;