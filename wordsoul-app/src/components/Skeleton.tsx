import React from 'react';

const Skeleton: React.FC<{ type: 'cards' | 'bar' }> = ({ type }) => {
  if (type === 'cards') {
    return (
      <div className="container mx-auto w-7/12 grid grid-cols-3 gap-4">
        {[...Array(6)].map((_, i) => (  // Giả 6 cards
          <div key={i} className="animate-pulse">
            <div className="bg-gray-600 h-48 rounded"></div>
            <div className="bg-gray-600 h-4 mt-2 rounded w-3/4"></div>
            <div className="bg-gray-600 h-3 mt-1 rounded w-1/2"></div>
          </div>
        ))}
      </div>
    );
  }
  if (type === 'bar') {
    return (
      <div className="w-9/12 border-white border-2 rounded-lg p-3">
        {[...Array(10)].map((_, i) => (  // Giả 10 bars
          <div key={i} className="animate-pulse mb-4">
            <div className="bg-gray-600 h-12 rounded"></div>
          </div>
        ))}
      </div>
    );
  }
  return null;
};

export default Skeleton;