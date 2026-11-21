cmake_minimum_required(VERSION 3.16)
project(chromawebp LANGUAGES C)

add_library(chromawebp SHARED "${CMAKE_CURRENT_LIST_DIR}/../chroma_webp.c")
target_include_directories(chromawebp PRIVATE "${WEBP_INCLUDE_DIR}")
# demux → webp → sharpyuv
target_link_libraries(chromawebp PRIVATE "${WEBPDEMUX_LIBRARY}" "${WEBP_LIBRARY}")
if(SHARPYUV_LIBRARY)
    target_link_libraries(chromawebp PRIVATE "${SHARPYUV_LIBRARY}")
endif()
set_target_properties(
    chromawebp
    PROPERTIES OUTPUT_NAME chromawebp C_VISIBILITY_PRESET hidden
)

if(NOT WIN32)
    find_package(Threads REQUIRED)
    target_link_libraries(chromawebp PRIVATE Threads::Threads m)
endif()

if(APPLE)
    set_target_properties(chromawebp PROPERTIES INSTALL_NAME_DIR "@rpath" BUILD_RPATH "@loader_path")
endif()
