// import { Link } from "react-router-dom";
// import HeroSection from "../../components/HeroSection";

// const VocabularySet: React.FC = () => {
//   return (
//     <>
//       <HeroSection
//         title="Welcome to Elaris"
//         description="Hành trình giải mã những văn tự cố, giải thoát những sinh vật bí ẩn, xây dựng kiến thức lâu dài."
//         textButton="Bắt đầu"
//         image="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756296491/vocabulary_sets/banner1_cgzmcx.gif"
//         bottomImage="./src/assets/grass.gif"
//         hidden={true}
//       />

//       <div className="bg-black text-white h-auto w-full flex justify-center items-center">
//         <div className="w-7/12 flex items-start gap-5 py-10">
//           {/* Bên trái  */}
//           <div className="w-9/12 border-white border-2 rounded-lg p-3">
//             {/* Thanh từ vựng */}
//             <div className="flex justify-between items-start mb-2 border-b-2 border-white pb-3 font-pixel">
//               {/* Flex column Word, loai tu, phat am,  */}
//               <div className="flex flex-col gap-1 mb-4">
//                 {/* Word */}
//                 <div className="text-3xl ">Human</div>
//                 {/* Loai tu */}
//                 <div className="text-s ">Noun</div>
//                 {/* Phat am */}
//                 <div className="text-s ">/adsf/</div>
//               </div>
//               {/* Nghĩa */}
//               <div className="text-2xl ">Con người</div>
//               {/* Ảnh minh họa */}
//               <div className="w-40 h-30">
//                 <img
//                   src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756296491/vocabulary_sets/banner1_cgzmcx.gif"
//                   alt="minh hoa"
//                   className="w-full h-full object-cover rounded-lg"
//                 />
//               </div>
//             </div>
            
//           </div>
//           {/* Bên phải */}
//           <div className="w-3/12 flex flex-col gap-3">
//             {/* Profile */}
//             <div className="flex flex-col gap-3 items-center border-2 border-white rounded-lg p-3">
//               {/* profile box */}
//               <div>
//                 <div className="flex gap-2 mb-3">
//                   <div className="w-20 h-20">
//                     <img
//                       src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756296491/vocabulary_sets/banner1_cgzmcx.gif"
//                       alt="minh hoa"
//                       className="w-full h-full object-cover"
//                     />
//                   </div>
//                   <div>
//                     <div>Name</div>
//                     <div>Level</div>
//                   </div>
//                 </div>
//                 <Link to="/signup" className="no-underline">
//                   <button className="relative flex items-center justify-center w-full px-2 py-1.5 bg-yellow-300 text-black rounded-xs hover:bg-yellow-200 custom-cursor">
//                     <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
//                     <span className="mx-1 text-xs font-bold font-sans">View Profile</span>
//                     <span className="absolute right-0 w-0.5 h-full bg-yellow-500" />
//                     <span className="absolute top-6.5 right-0 w-full h-1 bg-yellow-500" />
//                     <span className="absolute bottom-6.5 right-0 w-full h-0.5 bg-yellow-500" />
//                   </button>
//                 </Link>
//               </div>

//             </div>
//             {/* Progress */}
//             <div className="flex flex-col gap-3 items-center border-2 border-white rounded-lg p-3">
//               <div>Progress</div>
//               <div>Hello</div>
//             </div>
//           </div>
//         </div>
//       </div>
//     </>
//   )
// };
// export default VocabularySet;