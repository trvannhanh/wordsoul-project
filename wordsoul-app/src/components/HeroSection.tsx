interface HeroProps {
    title: string;
    description: string;
    hidden?: boolean; // Made optional with default value
    image?: string; // Background image URL
    textButton: string;
    bottomImage?: string; // Bottom background image URL
    height?: string; // New prop for minimum height
}

const HeroSection: React.FC<HeroProps> = ({ title, description, hidden = false, image = './src/assets/thumb.gif', textButton, bottomImage = './src/assets/grass.gif', height = '15rem', }) => {
    return (
        // truyền props image vào background
        <div className={`min-h-[15rem] max-h-[50rem] mb-10 bg-auto bg-center flex items-center justify-center relative top-[3.25rem]`} style={{ backgroundImage: `url(${image})`,  height: `${height}`}} >
            <div className="absolute inset-0 bg-black/50"></div>
            <div className="relative text-center text-white">
                <div className="max-w-md px-4 flex flex-col items-center text-center">
                    {/* truyền props title vào h1 */}
                    <h1 className="mb-5 text-xl font-pixel">{title}</h1>
                    {/* truyền props hidden vào image để ẩn*/}
                    {!hidden && (
                        <img
                            src="./src/assets/title.gif"
                            alt="Hero title"
                            className="w-2xs"
                        />
                    )}
                    {/* truyền props description vào p */}
                    <p className="mb-5 text-lg font-extralight">{description}</p>
                    {!hidden && (
                        <button className="relative flex items-center px-20 py-3 bg-yellow-300 text-black rounded-md hover:bg-yellow-200 custom-cursor z-999">
                            <span className="absolute left-0 w-1 h-full bg-yellow-500" />
                            {/* truyền props text vào span */}
                            <span className="mx-2 text-lg font-pixel">{textButton}</span>
                            <span className="absolute right-0 w-1 h-full bg-yellow-500" />
                            <span className="absolute top-12 right-0 w-full h-2 bg-yellow-500" />
                            <span className="absolute bottom-13 right-0 w-full h-0.5 bg-yellow-500" />
                        </button>
                    )}
                </div>
            </div>
            {/* tryền props ẩn background bên dưới */}
            {!hidden && (
                <div
                    className="h-80 w-full bottom-0 bg-auto absolute"
                    style={{ backgroundImage: `url(${bottomImage})` }}
                ></div>
            )}
        </div>
    )
};

export default HeroSection;


